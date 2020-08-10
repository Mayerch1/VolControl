using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;

namespace VolControl
{

    class Program
    {

        public static Joystick GetStick(string refGuid)
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;


            foreach (var deviceInstance in directInput.GetDevices())
            {

                if (deviceInstance.ProductGuid.ToString() == refGuid)
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                    break;
                }
            }


            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("Specified GUID not found.");
                return null;
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);
            Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);


            // if more than 16 changes occur during samples
            // the oldest changes get lost
            joystick.Properties.BufferSize = 8;

            return joystick;
        }


        public static void ProcessStick(StickData stick)
        {
            if(stick.currentState == null || stick.lastState == stick.currentState)
            {
                return; // no changes
            }



            MediaControl.Potentiometer(7, stick.mode, stick.lastState.X, stick.currentState.X);
            MediaControl.Potentiometer(6, stick.mode, stick.lastState.Y, stick.currentState.Y);


            MediaControl.Potentiometer(0, stick.mode, stick.lastState.Z, stick.currentState.Z);
            MediaControl.Potentiometer(1, stick.mode, stick.lastState.RotationX, stick.currentState.RotationX);

            MediaControl.Potentiometer(2, stick.mode, stick.lastState.RotationY, stick.currentState.RotationY);
            MediaControl.Potentiometer(3, stick.mode, stick.lastState.RotationZ, stick.currentState.RotationZ);


            MediaControl.MicSwitch(stick.lastState.Buttons[0], stick.currentState.Buttons[0]);





            stick.lastState = stick.currentState;
            stick.currentState = null;
        }


        public static void InitStick(StickData stick)
        {
            // enforce mute, when is muted
            // TODO: implement
            Console.WriteLine(stick.lastState);
        }



        public static void FailSafe()
        {
            MediaControl.MicSwitch(true, false);
            SystemSounds.Asterisk.Play();
        }


        static void Main(string[] args)
        {

            for(int i=0; i<Settings.inputSticks.Count; i++)
            {
                Settings.inputSticks[i].stick = GetStick(Settings.inputSticks[i].guid);
            }


            // safe-remove all controllers which are not plugged in
            for(int i = Settings.inputSticks.Count - 1; i >= 0; i--)
            {
                if(Settings.inputSticks[i].stick is null)
                {
                    Settings.inputSticks.RemoveAt(i);
                }
            }


            foreach(var stick in Settings.inputSticks)
            {
                // is null if not found
                stick.stick.Acquire();

                stick.lastState = new JoystickState();
                stick.stick.GetCurrentState(ref stick.lastState);

                InitStick(stick);
            }


            while (true)
            {
                foreach(var stick in Settings.inputSticks)
                {
                    stick.currentState = new JoystickState();

                    try
                    {
                        stick.stick.GetCurrentState(ref stick.currentState);

                    }
                    catch(SharpDX.SharpDXException ex)
                    {
                        Console.WriteLine(ex);
                        FailSafe();
                        return;
                    }
                    
                    ProcessStick(stick);
                }
                Thread.Sleep((1000 / Settings.pollRateHz));
            }


            // give time to flush interface to voiceMeter
            Thread.Sleep(250);
        }
    }
}

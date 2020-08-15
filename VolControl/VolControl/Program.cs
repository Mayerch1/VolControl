using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Reflection;
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


        /// <summary>
        /// Connect all connected sticks out of the inputSticks list
        /// </summary>
        public static void AquireSticks()
        {

            // don't do anything if every controller in the list is already connected
            // and if no prev. connected controller was disconnected
            if(Settings.inputSticks.Count == Settings.stickMap.Count)
            {
                return;
            }


            // try to connect all not-yet connected sticks
            // do not 'touch' sticks which are fully recognised

            foreach(var pair in Settings.stickMap)
            {
                string guid = pair.Key;


                // check if the guid is already in use and ignore it
                if (Settings.inputSticks.ContainsKey(guid)) { 
                    continue;
                }


                Joystick joystick = GetStick(guid);

                if(joystick != null)
                {
                    StickData stickData = new StickData() { stick = joystick };

                    // activate the stick
                    stickData.stick.Acquire();

                    Settings.inputSticks.Add(guid, stickData);
                }
            }
        }





        public static void ProcessSticks()
        {

            // if a single device reports mute, it is overriding all others
            // therefore this is only set after the loop iteration
            // -- however ppt is overriding mute
            bool is_mute = false;
            bool is_ppt = false;

            // toggle mute does NOT override is_mute or ppt
            // however it is inverting the mute status
            bool toggle_mute = false;

            // other settings are overriden based on the saved order

            foreach (var pair in Settings.inputSticks)
            {
                StickData stick = pair.Value;

                //stick.currentState.

                if (stick.currentState == null)
                {
                    continue; // readout failed somehow (uncaught failure), shouldn't really occur
                }
                else if (stick.lastState == null)
                {
                    // stick was connected for first time
                    stick.lastState = stick.currentState;
                    stick.currentState = null;
                    continue;
                }
                else if (stick.currentState == stick.lastState)
                {
                    continue; // no update
                }

                if (pair.Value.lastState == pair.Value.currentState)
                {
                    continue;
                }



                // look up mapping for this device
                // key MUST exist (otherwise inputStick wouldn't list this key
                StickMapping mapping = Settings.stickMap[pair.Key];


                if (mapping.mute_switch is int mute_switch)
                {

                    // inverted logic, as mic is only active when signal is present
                    bool is_muted_switch = !stick.currentState.Buttons[mute_switch];

                    is_mute |= is_muted_switch;
                }



                // NOTE: ppt is useless when no other mute button is bined
                if(mapping.ppt is int ppt_btn)
                {
                    is_ppt = stick.currentState.Buttons[ppt_btn];
                }



                if(mapping.mute_toggle is int mute_toggle)
                {
                    bool is_mute_toggled = stick.currentState.Buttons[mute_toggle];
                    bool is_last_mute_toggled = stick.lastState.Buttons[mute_toggle];

                    if (is_last_mute_toggled != is_mute_toggled)
                    {
                        toggle_mute |= is_mute_toggled;
                    }
                }



                // handle all potentiometers
                foreach(var slider in mapping.slider)
                {
                    var lastAxis = stick.GetLastStateByString(slider.Button);
                    var currAxis = stick.GetCurrentStateByString(slider.Button);


                    if(lastAxis is int && currAxis is int)
                    {
                        MediaControl.Potentiometer(slider.index, (int)lastAxis, (int)currAxis);
                    }
                    else
                    {
                        Console.WriteLine("ERR");
                    }
                }

                stick.lastState = stick.currentState;
                stick.currentState = null;

            }



            MediaControl.MicStateMachine(is_ppt, is_mute, toggle_mute);
        }



        public static void FailSafe()
        {
            MediaControl.MicSwitch(false);
            SystemSounds.Asterisk.Play();
        }


        static void Main(string[] args)
        {
            AquireSticks();

            
            int elapsed_ms = 0;


            while (true)
            {
                foreach(var pair in Settings.inputSticks)
                {
                    StickData stick = pair.Value;


                    // cleanout the entire state, as GetCurrentState must not necesarrily override the 'ref'
                    stick.currentState = new JoystickState();

                    try
                    {
                        stick.stick.GetCurrentState(ref stick.currentState);

                    }
                    catch(SharpDX.SharpDXException ex)
                    {
                        Console.WriteLine(ex);
                        FailSafe();

                        // gc will delete StickData when not referenced anymore
                        Settings.inputSticks.Remove(pair.Key);


                        // modifying iterated list demands abort of foreach
                        // otherwise access error can occur
                        break;
                    }
                }

                // as some buttons can override (like mute)
                // all HIDs must be processed at the same time
                ProcessSticks();


                int wait_time = 1000 / Settings.pollRateHz;
                Thread.Sleep(wait_time);
                elapsed_ms += wait_time;


                // scan for new controllers every 30 seconds
                // this helps when e.g. a wheel with mute button is plugged in after boot
                if(elapsed_ms > 30000)
                {
                    elapsed_ms = 0;
                    AquireSticks();

                }
            }

        }
    }
}

﻿using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Text;

namespace VolControl
{
    public static class DevicePolling
    {
        private static Joystick GetStick(string refGuid)
        {

            // Initialize DirectInput
            var directInput = new DirectInput();
            var targetGuid = new Guid(refGuid);

            Joystick joystick;
            try
            {
                joystick = new Joystick(directInput, targetGuid);
                joystick.Properties.BufferSize = 8; // for getting events since last poll
            }
            catch
            {
                // joystick with this guid is not connected
                Console.WriteLine("Joystick with [" + refGuid + "] not connected");
                joystick = null;
            }

            return joystick;
        }


        /// <summary>
        /// Connect all connected sticks out of the inputSticks list
        /// </summary>
        public static int AquireSticks()
        {
            int addedSticks = 0;
            int oldConnectedCount = Settings.inputSticks.Count;

            // don't do anything if every controller in the list is already connected
            // and if no prev. connected controller was disconnected
            if (oldConnectedCount == Settings.stickMap.Count)
            {
                return addedSticks;
            }


            // try to connect all not-yet connected sticks
            // do not 'touch' sticks which are fully recognised

            foreach (var pair in Settings.stickMap)
            {
                string guid = pair.Key;


                // check if the guid is already in use and ignore it
                if (Settings.inputSticks.ContainsKey(guid))
                {
                    continue;
                }


                Joystick joystick = GetStick(guid);

                if (joystick != null)
                {
                    StickData stickData = new StickData() { stick = joystick };

                    // activate the stick
                    stickData.stick.Acquire();

                    Settings.inputSticks.Add(guid, stickData);
                    addedSticks++;
                }
            }

            return addedSticks;
        }


        /// <summary>
        /// Uses previous and current stickData
        /// sets microphone and potentiometer (volume) status based on them
        /// </summary>
        public static void ProcessSticks()
        {
            // if a single device reports mute, it is overriding all others
            // therefore this is only set after the loop iteration
            // -- however ppt is overriding mute
            bool[] is_mute = new bool[8];
            bool[] is_ppt = new bool[8];

            // toggle mute does NOT override is_mute or ppt
            // however it is inverting the mute status
            bool[] toggle_mute = new bool[8];

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

                foreach(var muteSwitch in mapping.muteSwitches)
                {
                    // inverted logic, as mic is only active when signal is present
                    bool is_muted_switch = !stick.currentState.Buttons[muteSwitch.index];
                    is_mute[muteSwitch.lane] |= is_muted_switch;
                }


                // NOTE: ppt is useless when no other mute button is bined
                foreach (var pptSwitch in mapping.ppts)
                {
                    // inverted logic, as mic is only active when signal is present
                    is_ppt[pptSwitch.lane] |= stick.currentState.Buttons[pptSwitch.index];
                }


                foreach (var muteToggle in mapping.muteToggles)
                {
                    bool is_mute_toggled = stick.currentState.Buttons[muteToggle.index];
                    bool is_last_mute_toggled = stick.lastState.Buttons[muteToggle.index];

                    if (is_last_mute_toggled != is_mute_toggled)
                    {
                        toggle_mute[muteToggle.lane] |= is_mute_toggled;
                    }
                }



                // handle all potentiometers
                foreach (var slider in mapping.slider)
                {
                    var lastAxis = stick.GetLastStateByString(slider.Button);
                    var currAxis = stick.GetCurrentStateByString(slider.Button);


                    // only do something when Axis changed more than 10
                    if (lastAxis is int && currAxis is int && Math.Abs((int)lastAxis - (int)currAxis) > 10)
                    {
                        MediaControl.Potentiometer(slider.index, (int)lastAxis, (int)currAxis);
                    }
                }


                // handle all sound triggers
                foreach(var trigger in mapping.soundTriggers)
                {
                    var lastTrigger = stick.lastState.Buttons[trigger.index];
                    var currentTrigger = stick.currentState.Buttons[trigger.index];

                    if(currentTrigger && !lastTrigger)
                    {
                        // replay the matching file, will abort all other replays
                        MediaControl.TogglePlayFile(trigger.soundFile);
                    }

                }

                stick.lastState = stick.currentState;
                stick.currentState = null;

            }



            MediaControl.MicStateMachine(is_ppt, is_mute, toggle_mute);
        }
    }
}

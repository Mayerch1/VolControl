﻿using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;

namespace VolControl
{
    public static class MediaControl
    {

        private static bool loggedIn;
        private static bool is_muted = false;
        private static bool ptt_active = false;

        private static bool toggle_override = false;
        private static bool ptt_override = false;



        private static SoundPlayer soundPlayer =  new SoundPlayer();

        private static void Login()
        {
            if (!loggedIn)
            {
                Int32 res = VoiceMeeterRemoteAPI.Login();
                if(res == 0)
                {
                    loggedIn = true;
                    SyncSettings();
                }
            }

        }


        private static void Logout()
        {
            if (loggedIn)
            {
                VoiceMeeterRemoteAPI.Logout();
                loggedIn = false;
            }
        }



        private static void SyncSettings()
        {
            VoiceMeeterRemoteAPI.GetMute(Settings.micLane, ref MediaControl.is_muted);
        }


        private static void PlaySound(string file)
        {
            soundPlayer.SoundLocation = file;
            soundPlayer.PlaySync();
        }


        private static float Map(int value, float fromSource, float toSource, float fromTarget, float toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }



        public static bool GetMuteStatus()
        {
            return is_muted;
        }
        public static bool GetPttStatus()
        {
            return ptt_active;
        }


        public static void Potentiometer(int id, int last, int current)
        {
            if(last == current)
            {
                return;
            }


            Login();


            float db;


            // mid point is at 3/4 of poti
            // this is 768 in arduino [0, 1024] scale
            // resulting in 49199 in [0, 65535] scale
            if(current == 49134)
            {
                // arduino introduces deadzone around this value
                db = 0; // neutral position
            }
            else if(current > 49134)
            {
                db = Map(current, 52000, 65535, 0, 12);
            }
            else
            {
                // voicemeeter supports down to -60
                // however -40 seems to be enough for now (-> higher precision on knob)
                db = Map(current, 0, 46000, -40, 0);
            }


            VoiceMeeterRemoteAPI.SetGain(id, db);
            //onsole.WriteLine(String.Format("Set gain of {0} to {1}", id, db));
        }


        /// <summary>
        /// Unmute/Mute microphone, might double-mute when sample rate is too high
        /// Checks with current state of Voicemeeter and acts on change
        /// </summary>
        /// <param name="is_mute">true for muting the mic</param>
        public static void MicSwitch(bool is_mute)
        {

            Login();

            if (is_mute)
            {
                Login();
                var succes = VoiceMeeterRemoteAPI.SetMute(Settings.micLane, true);
                if (succes == 0)
                {
                    PlaySound(Settings.MuteSound);
                }
                else
                {
                    SystemSounds.Asterisk.Play();
                }
            }
            else
            {
                Login();
                var succes = VoiceMeeterRemoteAPI.SetMute(Settings.micLane, false);

                if (succes == 0)
                {
                    PlaySound(Settings.UnMuteSound);
                }
                else
                {
                    SystemSounds.Asterisk.Play();
                }
            }

            // wait until command is received
            //Thread.Sleep(1000);
        }



        public static void MicStateMachine(bool ppt, bool mute_switch, bool mic_toggle)
        {
            if (!loggedIn)
            {
                // Login does check for loggedIn, but this saves call overhead
                // (this loop is executed at every iteration)
                Login();
            }


            // ppt is overriding everything
            if(ppt && is_muted)
            {
                MicSwitch(false);
                is_muted = false;

                ptt_active = true;
                ptt_override = true;

                toggle_override = false;
                return;
            }
            else if(ppt && !is_muted)
            {
                // ensures that mic is muted on ppt release 
                // even when mic wasn't muted before
                ptt_active = true;
                ptt_override = true;

                toggle_override = false;
                return;
            }
            else if(!ppt && ptt_active)
            {
                if (!is_muted)
                {
                    // ppt forces into mute state, ignoring other values
                    MicSwitch(true);
                    is_muted = true;
                }

                // keep override active
                ptt_active = false;
                return;
            }



            // toggle cannot override mute_switch in mute state
            if(mute_switch)
            {
                ptt_override = false;
                toggle_override = false;

                if (!is_muted)
                {
                    MicSwitch(true);
                    is_muted = true;
                }

                return;
            }
            // when mute switch is off toggle could override
            else if(!mute_switch && !toggle_override && !ptt_override && is_muted)
            {
                MicSwitch(false);
                is_muted = false;
                return;
            }
            else if (mic_toggle)
            {
                is_muted = !is_muted;
                MicSwitch(is_muted);

                ptt_override = false;
                toggle_override = true;
                return;
            }
        }



        /// <summary>
        /// Plays the specified file (if it exists)
        /// If voicemeeter is already playing a file, it is stopped
        /// </summary>
        /// <param name="file">path to the file, can be null to stop the replay</param>
        public static void TogglePlayFile(string file)
        {
            Int32 sucess;
            // turn off any atcive replay
            if(file == null)
            {
                sucess = VoiceMeeterRemoteAPI.SetRecorderPlaying(false);

                if(sucess != 0)
                {
                    SystemSounds.Asterisk.Play();
                }

                return;
            }



            if (!File.Exists(file))
            {
                SystemSounds.Asterisk.Play();
                return;
            }

            sucess = VoiceMeeterRemoteAPI.SetRecorderFile(file);

            if (sucess != 0)
            {
                SystemSounds.Asterisk.Play();
                return;
            }

            sucess = VoiceMeeterRemoteAPI.SetRecorderPlaying(true);
            

            if(sucess != 0)
            {
                SystemSounds.Asterisk.Play();
            }
        }



    }
}

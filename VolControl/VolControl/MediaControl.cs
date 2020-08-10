using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Text;

namespace VolControl
{
    public static class MediaControl
    {
        /// <summary>
        /// Determins which strip is controlled by the potentiometers
        /// </summary>
        public enum Mode
        {
            GH, // discord, default (out)
            AB // mic, vcable (in)
            
        }


        private static bool loggedIn;
        private static SoundPlayer soundPlayer =  new SoundPlayer();

        private static void Login()
        {
            if (!loggedIn)
            {
                Int32 res = VoiceMeeterRemoteAPI.Login();
                if(res == 0)
                {
                    loggedIn = true;
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


        private static void PlaySound(string file)
        {
            soundPlayer.SoundLocation = file;
            soundPlayer.PlaySync();
        }


        private static float Map(this int value, float fromSource, float toSource, float fromTarget, float toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }



        public static void Potentiometer(int id, Mode mode, int last, int current)
        {
            if(last == current)
            {
                return;
            }


            Login();


            float db = 0;

            // mid point is at 3/4 of poti
            // 0 => 40 151
            // -- => 39 000-
            // ++ => 41 000+
            if(current >= 39000 && current <= 41000)
            {
                db = 0; // neutral position
            }
            else if(current > 41000)
            {
                db = Map(current, 41000, 65535, 0, 12);
            }
            else
            {
                // voicemeeter supports down to -60
                // however -40 seems to be enough for now (-> higher precision on knob)
                db = Map(current, 0, 39000, -40, 0);
            }


            VoiceMeeterRemoteAPI.SetGain(id, db);
            //onsole.WriteLine(String.Format("Set gain of {0} to {1}", id, db));
        }


        public static void MicSwitch(bool lastState, bool currentState)
        {
            if(lastState == currentState)
            {
                return;
            }


            if (currentState)
            {
                Login();
                VoiceMeeterRemoteAPI.Mute(Settings.micLane, false);
                PlaySound(Settings.UnMuteSound);
            }
            else
            {
                Login();
                VoiceMeeterRemoteAPI.Mute(Settings.micLane, true);
                PlaySound(Settings.MuteSound);
            }
        }

    }
}

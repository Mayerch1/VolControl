using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Text;

namespace VolControl
{

    public class StickData
    {
        public string guid = null;

        public Joystick stick = null;
        public JoystickState lastState = null;
        public JoystickState currentState = null;

        // toggle the usage of potentaiometers
        public MediaControl.Mode mode = MediaControl.Mode.GH;

        /* list of button mapping */
        
    }


    public static class Settings
    {
        public static string MuteSound = @"F:\Christian\Documents\Code\Arduino\VolControl\VolControl\VolControl\res\mute.wav";
        public static string UnMuteSound = @"F:\Christian\Documents\Code\Arduino\VolControl\VolControl\VolControl\res\unMute.wav";


        public const int pollRateHz = 20;
        public const int micLane = 0;



        public static List<StickData> inputSticks = new List<StickData> {
            new StickData {
                guid = "80372341-0000-0000-0000-504944564944"
            },
            //new StickData
            //{
            //    guid = "183b0eb7-0000-0000-0000-504944564944"
            //},
            //new StickData
            //{
            //   guid = "0404044f-0000-0000-0000-504944564944"
            //}
            };

    }
}

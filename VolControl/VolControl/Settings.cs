using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace VolControl
{

    public class StickData
    {
        public Joystick stick = null;
        public JoystickState lastState = null;
        public JoystickState currentState = null;


        /* list of button mapping */

        private (MethodInfo?, ParameterInfo[]) getPropertyInvokable(string propertyName)
        {
            PropertyInfo propBind = typeof(JoystickState).GetProperty(propertyName);
            var propParam = propBind.GetMethod.GetParameters();

            return (propBind.GetMethod, propParam);
        }

        public object GetCurrentStateByString(string propertyName)
        {
            (MethodInfo, ParameterInfo[]) invokable = getPropertyInvokable(propertyName);

            return invokable.Item1.Invoke(this.currentState, invokable.Item2);
        }

        public object GetLastStateByString(string propertyName)
        {
            (MethodInfo, ParameterInfo[]) invokable = getPropertyInvokable(propertyName);

            return invokable.Item1.Invoke(this.lastState, invokable.Item2);
        }
    }




    public class StickMapping
    {
        public class Slider
        {
            public int index;
            public string Button;
        }

        public class SoundTrigger
        {
            public int index;
            public string soundFile;
        }

        public class MuteBtn
        {
            // Button index, cannot be an axis
            public int index;
            public int lane;
        }


        public List<MuteBtn> muteSwitches = new List<MuteBtn>();
        public List<MuteBtn> ppts = new List<MuteBtn>(); // push-to-talk
        public List<MuteBtn> muteToggles = new List<MuteBtn>();

        public List<Slider> slider = new List<Slider>();
        public List<SoundTrigger> soundTriggers = new List<SoundTrigger>();
    }


    public static class Settings
    {
        public static string MuteSound = "res/mute.wav";
        public static string UnMuteSound = "res/unMute.wav";


        public const int pollRateHz = 10;


        // settings for each stick, used for state machine
        public static Dictionary<string, StickMapping> stickMap = new Dictionary<string, StickMapping>();
        
        // directX object, used for polling
        public static Dictionary<string, StickData> inputSticks = new Dictionary<string, StickData>();

    }
}

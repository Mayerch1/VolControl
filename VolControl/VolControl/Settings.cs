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


        // Button index, cannot be an axis
        public int? mute_switch = null;
        public int? ppt = null; // push-to-talk
        public int? mute_toggle = null;

        public Slider[] slider;
    }


    public static class Settings
    {
        public static string MuteSound = @"F:\Christian\Documents\Code\Arduino\VolControl\VolControl\VolControl\res\mute.wav";
        public static string UnMuteSound = @"F:\Christian\Documents\Code\Arduino\VolControl\VolControl\VolControl\res\unMute.wav";


        public const int pollRateHz = 10;
        public const int micLane = 0;



        public static readonly Dictionary<string, StickMapping> stickMap = new Dictionary<string, StickMapping>
        {
            {"80372341-0000-0000-0000-504944564944", 
                new StickMapping{

                    mute_switch =0,

                    slider = new StickMapping.Slider[]{
                        new StickMapping.Slider
                        {
                            index =7,
                            Button = "X"
                        },
                        new StickMapping.Slider
                        {
                            index = 6,
                            Button = "Y"
                        },
                        new StickMapping.Slider
                        {
                            index = 0,
                            Button = "Z"
                        },
                        new StickMapping.Slider
                        {
                            index = 1,
                            Button = "RotationX"
                        },
                        new StickMapping.Slider
                        {
                            index = 2,
                            Button = "RotationY"
                        },
                        new StickMapping.Slider
                        {
                            index = 3,
                            Button = "RotationZ"
                        }

                    }
                } 
            },
            
            {"b66e044f-0000-0000-0000-504944564944", 
                new StickMapping{
                    mute_toggle = 4,
                }
            }
        };



        public static Dictionary<string, StickData> inputSticks = new Dictionary<string, StickData>();

    }
}

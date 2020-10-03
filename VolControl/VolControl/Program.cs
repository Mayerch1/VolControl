using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading;
using System.Xml;

namespace VolControl
{

    class Program
    {
        public static void ReloadSettings()
        {
            Settings.inputSticks.Clear();
            bool valid = LoadSettings();
            WindowsTrayIcon.UpdateValidConfigFlag(valid);

            DevicePolling.AquireSticks();
            WindowsTrayIcon.UpdateTrayIconList(Settings.inputSticks, Settings.stickMap.Count);
        }


   


        /// <summary>
        /// Reload the config from Settings.json
        /// Invokes Notification on error
        /// Tries to load when errors in format are present, but still returns failure then
        /// </summary>
        /// <returns>false on first failure, even when some settings are loaded</returns>
        public static bool LoadSettings()
        {
            bool validConfig = true;

            if (!File.Exists("Settings.json"))
            {
                return false;
            }

            var json = System.IO.File.ReadAllText("Settings.json");

            JArray parsed;
            try
            {
                 parsed = JArray.Parse(json);
            }
            catch
            {
                WindowsTrayIcon.InvalidConfigWarning("Invalid Json detected.");
                return false;
            }

            // clear old settings (otherwise deletion on config change not possible)
            Settings.stickMap.Clear();
            

            foreach(var device in parsed)
            {
                StickMapping map = new StickMapping();

                if(!(device is JObject))
                {
                    WindowsTrayIcon.InvalidConfigWarning("Invalid Parammeters are used");
                    validConfig = false;
                    continue;
                }


                string guid = (string)device["guid"];

                
                if(guid != null)
                {
                    if (Settings.stickMap.ContainsKey(guid))
                    {
                        // skip this config entry
                        WindowsTrayIcon.InvalidConfigWarning("Guid " + guid + " is used multiple times.");
                        validConfig = false;
                        continue;
                    }


                    // toggle switches can be null (not-set)
                    map.ppt = (int?)device["ptt_button"];
                    map.mute_switch = (int?)device["mute_switch"];
                    map.mute_toggle = (int?)device["mute_toggle"];


                    // iterate over slider array
                    // make sure that all necessary properties are set
                    var slidersObj = device["sliders"];
                    if(slidersObj is JArray sliders)
                    {
                        foreach(var slider in sliders)
                        {
                            int? idx = (int?)slider["index"];
                            string btn = (string)slider["button"];


                            // guarantee not null
                            if(idx != null && btn != null)
                            {
                                map.slider.Add(new StickMapping.Slider
                                {
                                    index = (int)idx,
                                    Button = btn
                                });

                            }

                        }
                    }


                    // iterate over all sound triggers
                    // same procedure as for sliders
                    var triggerObjs = device["sound_triggers"];
                    if(triggerObjs is JArray triggers)
                    {
                        foreach(var trigger in triggers)
                        {
                            int? idx = (int?)trigger["index"];
                            string file = (string)trigger["file"];

                            if(idx != null && file != null)
                            {
                                map.soundTriggers.Add(new StickMapping.SoundTrigger
                                {
                                    index = (int)idx,
                                    soundFile = file
                                });
                            }
                        }
                    }

                    Settings.stickMap.Add(guid, map);
                }

            }

            return validConfig;
        }



        public static void FailSafe()
        {
            MediaControl.MicSwitch(true);
            // failSafe passes state machine, therefore muted icon must be forced
            WindowsTrayIcon.UpdateTrayIconMic(true, false);
        }



        static void Main()
        {
            // OS specific code in this helper
            // trayIcon runs in other thread, but can  call these handles
            WindowsTrayIcon.Init(ReloadSettings);
            Thread.Sleep(100); // wait for notifyThread to be initalised (maybe replace with proper wait)

            bool validConf = LoadSettings();
            WindowsTrayIcon.UpdateValidConfigFlag(validConf);


            int loadedSticks = DevicePolling.AquireSticks();

            // do not show notification on app start
            // (only on error)
            if(loadedSticks == 0)
            {
                WindowsTrayIcon.NoStickWarning(Settings.stickMap.Count);
            }
            else
            {
                WindowsTrayIcon.UpdateTrayIconList(Settings.inputSticks, Settings.stickMap.Count);
            }
            
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
                    catch(SharpDX.SharpDXException)
                    {
                        FailSafe();

                        // gc will delete StickData when not referenced anymore
                        Settings.inputSticks.Remove(pair.Key);

                        // no more than 1 device can be disconnected at a time
                        WindowsTrayIcon.UpdateTrayIconList(Settings.inputSticks, Settings.stickMap.Count);
                        WindowsTrayIcon.RmController(1, Settings.inputSticks.Count, Settings.stickMap.Count);

                        // modifying iterated list demands abort of foreach
                        // otherwise access error can occur
                        break;
                    }
                }

                // as some buttons can override (like mute)
                // all HIDs must be processed at the same time
                DevicePolling.ProcessSticks();


                int wait_time = 1000 / Settings.pollRateHz;
                Thread.Sleep(wait_time);
                elapsed_ms += wait_time;



                if(elapsed_ms > 1000)
                {
                    // update tray icon once per second (only when status changed)
                    WindowsTrayIcon.UpdateTrayIconMic(MediaControl.GetMuteStatus(), MediaControl.GetPttStatus());
                }

                // scan for new controllers every 30 seconds
                // this helps when e.g. a wheel with mute button is plugged in after boot
                if(elapsed_ms > 30000)
                {
                    elapsed_ms = 0;
                    int addedSticks = DevicePolling.AquireSticks();

                    if (addedSticks > 0)
                    {
                        WindowsTrayIcon.UpdateTrayIconList(Settings.inputSticks, Settings.stickMap.Count);
                        WindowsTrayIcon.AddControler(addedSticks, Settings.inputSticks.Count, Settings.stickMap.Count);
                    }
                }
            }

        }
    }
}

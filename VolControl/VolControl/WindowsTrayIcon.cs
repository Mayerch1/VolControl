using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

/*
 * https://ambilykk.com/2019/03/20/system-tray-icon-for-net-core-console-app/
 */


namespace VolControl
{
    static class WindowsTrayIcon
    {

        // invokes function in Program to reload the settings file
        public delegate void ReloadSettingsHandler();
        public static ReloadSettingsHandler ReloadSettings = null;



        private static NotifyIcon icon = null;
        private static readonly Icon defaultIcon = new System.Drawing.Icon("res/volume.ico");
        private static readonly Icon muteIcon = new System.Drawing.Icon("res/mic-mute.ico");


        private static bool lastMuteStatus = true;
        private static bool lastPttStatus = true;

        // this is used to faster update only the mic info
        private static string invalidConfigString = "";
        private static string micInfoString = "";
        private static string deviceListString = "";
            

        public static void Init(ReloadSettingsHandler reloadHandle)
        {

            ReloadSettings += reloadHandle;

            // tray icon must run in separate thread
            // otherwise context menu is not functioning

            System.Threading.Thread notifyThread = new System.Threading.Thread(
                delegate ()
                {
                    

                    var contextStrip = new ContextMenuStrip();

                    
                    contextStrip.Items.Add("Reload");
                    contextStrip.Items.Add("-");
                    contextStrip.Items.Add("Exit");
                    contextStrip.ItemClicked += Context_ItemClicked;

                    icon = new NotifyIcon
                    {
                        Icon = defaultIcon,
                        Visible = true,
                        BalloonTipTitle = "VolControl",
                        ContextMenuStrip = contextStrip
                    };

                    System.Windows.Forms.Application.Run();
                });

            notifyThread.Start();



            // force update to init the tooltip text
            // use mic-active and no ptt as default
            UpdateTrayIconMic(false, false);
        }



        private static void Context_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
         
            if(e.ClickedItem.Text == "Reload")
            {
                ReloadSettings?.Invoke();
            }
            else if(e.ClickedItem.Text == "Exit"){
                Environment.Exit(0);
            }
        }

        private static void UpdateIconText()
        {
            if(icon != null)
            {
                icon.Text = invalidConfigString + micInfoString + deviceListString;
            }
        }
       


        public static void UpdateTrayIconMic(bool isMuted, bool isPtt)
        {
            if(icon == null)
            {
                return;
            }

            if(lastMuteStatus != isMuted || lastPttStatus != isPtt)
            {
                micInfoString = String.Format("Muted: {0} - PTT: ", isMuted);

                if (isMuted)
                {
                    micInfoString = "Muted - ";
                    icon.Icon = muteIcon;
                }
                else
                {
                    micInfoString = "Not Muted - ";
                    icon.Icon = defaultIcon;
                }


                if (isPtt)
                {
                    micInfoString += "PTT on\n";
                }
                else
                {
                    micInfoString += "PTT off\n";
                }
              

                UpdateIconText();

                lastPttStatus = isPtt;
                lastMuteStatus = isMuted;
            }
        }



        public static void UpdateTrayIconList(Dictionary<string, StickData> sticks, int configCount)
        {
            if (icon != null)
            {
                icon.Text = micInfoString;

                deviceListString = String.Format("Connected {0}/{1} devices\n", sticks.Count, configCount);
                UpdateIconText();
            }
        }


        public static void UpdateValidConfigFlag(bool validConfig)
        {
            if (!validConfig)
            {
                invalidConfigString = "Invalid Config\n";
            }
            else
            {
                invalidConfigString = "";
            }


            UpdateIconText();
        }



        public static void InvalidConfigWarning(string description)
        {
            if(icon != null)
            {
                icon.BalloonTipTitle = "VolControl - Invalid Config Detected";
                icon.BalloonTipText = description;
                icon.ShowBalloonTip(2000);
            }
        }

        public static void NoStickWarning(int configCount)
        {
            if (icon != null)
            {
                icon.BalloonTipTitle = "VolContorl - No Device Found";
                icon.BalloonTipText = String.Format("0/{0} devices are active", configCount);
                icon.ShowBalloonTip(1000);
                
            }
        }


        public static void AddControler(int addedCount, int newConnected, int configCount)
        {
            if (icon != null)
            {
                icon.BalloonTipTitle = "VolControl - Device Connected";
                icon.BalloonTipText = String.Format("Added {0} device(s). {1}/{2} devices are active.", addedCount, newConnected, configCount);
                icon.ShowBalloonTip(1000);
            }
        }


        public static void RmController(int rmCount, int newConnected, int configCount)
        {
            if (icon != null)
            {
                icon.BalloonTipTitle = "VolControl - Removed Device";
                icon.BalloonTipText = String.Format("Removed {0} device(s). {1}/{2} devices are active.", rmCount, newConnected, configCount);
                icon.ShowBalloonTip(1000);
            }
        }
    }
}

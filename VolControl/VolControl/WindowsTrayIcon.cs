using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VolControl
{
    static class WindowsTrayIcon
    {
        private static NotifyIcon icon = null;
        private static Icon defaultIcon = new System.Drawing.Icon("res/volume.ico");
        private static Icon muteIcon = new System.Drawing.Icon("res/mic-mute.ico");


        private static bool lastMuteStatus = true;
        private static bool lastPttStatus = true;

        // this is used to faster update only the mic info
        private static string micInfoString = "";
        private static string deviceListString = "";
            

        public static void Init()
        {
            icon = new NotifyIcon();
            icon.Icon = defaultIcon;
            icon.Visible = true;
            icon.BalloonTipTitle = "VolControl";

            UpdateTrayIconMic(false, false);
        }



        public static void UpdateTrayIconMic(bool isMuted, bool isPtt)
        {
            if(lastMuteStatus != isMuted || lastPttStatus != isPtt)
            {
                micInfoString = String.Format("Muted: {0} - PTT: ", isMuted);
                if (isPtt)
                {
                    micInfoString += "Active\n";
                }
                else
                {
                    micInfoString += "Inactive\n";
                }
                if (isMuted)
                {
                    icon.Icon = muteIcon;
                }
                else
                {
                    icon.Icon = defaultIcon;
                }

                icon.Text = micInfoString + deviceListString;

                lastPttStatus = isPtt;
                lastMuteStatus = isMuted;
            }
        }


        public static void UpdateTrayIconList(Dictionary<string, StickData> sticks, int configCount)
        {
            icon.Text = micInfoString;

            deviceListString = String.Format("Connected {0}/{1} devices\n", sticks.Count, configCount);
            icon.Text = micInfoString + deviceListString;
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

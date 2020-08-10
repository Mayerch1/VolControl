using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace VolControl
{
    public class VoiceMeeterRemoteAPI
    {

        /// <summary>
        /// Search the registry for voicemeeter install
        /// If no registry install found, try to use local folder
        /// </summary>
        static VoiceMeeterRemoteAPI()
        {
            object result = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\VB:Voicemeeter {17359A74-1236-5467}", "UninstallString", null);


            if (result is null)
            {
                result = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\VB:Voicemeeter {17359A74-1236-5467}", "UninstallString", null);
            }



            string dllPath = "";
            if (result is string path)
            {
                dllPath = path.Remove(path.LastIndexOf('\\')) + '\\';
            }


            if (!Directory.Exists(dllPath))
            {
                Console.WriteLine("Cannot find dll " + dllPath);
            }

            else
            {
                SetDllDirectory(dllPath);
            }
        }

      


        public const string dllPath = "VoicemeeterRemote64.dll";


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);



        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 VBVMR_Login();


        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 VBVMR_Logout();

        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 VBVMR_GetParameterFloat([In] byte[] szParamName, ref float pValue);



        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 VBVMR_SetParameterFloat([In] byte[] szParamName, [In] float Value);



        private static byte[] StringToBytes(string str)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetBytes(str);
        }



        /// <summary>
        /// Connect to the internal server, must be called first
        /// </summary>
        /// <returns></returns>
        public static Int32 Login()
        {
            return VBVMR_Login();
        }


        /// <summary>
        /// logoff from the internal server
        /// </summary>
        /// <returns></returns>
        public static Int32 Logout()
        {
            return VBVMR_Logout();
        }


        public static Int32 SetParameterFloat(string name, float value)
        {
            byte[] buff = StringToBytes(name);
            Int32 res = VBVMR_SetParameterFloat(buff, value);

            return res;
        }


        //public static Int32 GetParameterFloat(string name)
        //{
        //    byte[] buff = StringToBytes(name);

        //    float gain = 0;


        //    Int32 res = VBVMR_GetParameterFloat(buff, ref gain);

        //    return res;
        //}



        /// <summary>
        /// Mute/Unmute the given channel
        /// </summary>
        /// <param name="channel">Audiostrip (1 offset)</param>
        /// <param name="is_mute">true for mute, false for unmute</param>
        /// <returns></returns>
        public static Int32 Mute(int channel, bool is_mute = true)
        {
            string name = "Strip[" + channel + "].Mute";
            float mute = is_mute ? 1 : 0;


            return SetParameterFloat(name, mute);
        }


        /// <summary>
        /// Set the gain of the specified channel
        /// </summary>
        /// <param name="channel">Audiostrip (1 offset)</param>
        /// <param name="gain">in dB, [-60, 12]</param>
        /// <returns></returns>
        public static Int32 SetGain(int channel, float gain = 0.0f)
        {
            string name = "Strip[" + channel + "].gain";

            return SetParameterFloat(name, gain);
        }



        public static Int32 GetGain(int channel, out float gain)
        {
            //string name = "Strip[0].gain";
            //byte[] buff = StringToBytes(name);


            //Int32 res = VBVMR_GetParameterFloat(buff, gain);

            //return res;
            gain = 0;
            return 0;
        }

    }
}

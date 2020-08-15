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

        [DllImport(dllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 VBVMR_GetParameterFloat([In] byte[] szParamName, ref float pValue);



        [DllImport(dllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 VBVMR_SetParameterFloat([In] byte[] szParamName, [In] float Value);


        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 VBVMR_IsParametersDirty();



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



        /// <summary>
        /// Set a generic parameter in Voicemeeter syntax
        /// </summary>
        /// <param name="name">query command</param>
        /// <param name="value">value to be set</param>
        /// <returns>status - 0 on success</returns>
        public static Int32 SetParameterFloat(string name, float value)
        {
            byte[] buff = StringToBytes(name);
            Int32 res = VBVMR_SetParameterFloat(buff, value);

            return res;
        }


        /// <summary>
        /// Get a generic parameter using Voicemeeter syntax
        /// </summary>
        /// <param name="name">query command</param>
        /// <param name="value">output parameter</param>
        /// <returns>status - 0 on success</returns>
        public static Int32 GetParameterFloat(string name, ref float value)
        {

            // must be called before reading a parameter
            // the result is ignored, as this handler is stateless
            var dirty = VBVMR_IsParametersDirty();

            byte[] buff = StringToBytes(name);

            // somehow the first readout is not reliable
            Int32 res = VBVMR_GetParameterFloat(buff, ref value);
            res = VBVMR_GetParameterFloat(buff, ref value);

            return res;
        }



        /// <summary>
        /// Mute/Unmute the given channel
        /// </summary>
        /// <param name="channel">Audiostrip (0 offset)</param>
        /// <param name="is_mute">true for mute, false for unmute</param>
        /// <returns>status - 0 on success</returns>
        public static Int32 SetMute(int channel, bool is_mute = true)
        {
            string name = "Strip[" + channel + "].Mute";
            float mute = is_mute ? 1 : 0;


            return SetParameterFloat(name, mute);
        }


        /// <summary>
        /// Get the mute status of the given channel
        /// </summary>
        /// <param name="channel">Audiostrip (0 offset)</param>
        /// <param name="is_mute">ref, true for mute, false for unmute</param>
        /// <returns>status - 0 on success</returns>
        public static Int32 GetMute(int channel, ref bool is_mute)
        {
            string name = "Strip[" + channel + "].Mute";

            float is_mute_fl = -1;

            
            Int32 status = GetParameterFloat(name, ref is_mute_fl);

            if (status == 0)
            {
                is_mute = is_mute_fl == 0 ? false : true;
            }

            return status;
        }


        /// <summary>
        /// Set the gain of the specified channel
        /// </summary>
        /// <param name="channel">Audiostrip (0 offset)</param>
        /// <param name="gain">in dB, [-60, 12]</param>
        /// <returns>status - 0 on success</returns>
        public static Int32 SetGain(int channel, float gain = 0.0f)
        {
            string name = "Strip[" + channel + "].gain";

            return SetParameterFloat(name, gain);
        }


        /// <summary>
        /// Get the gain of the specified channel
        /// </summary>
        /// <param name="channel">Audiostrip (0 offset)</param>
        /// <param name="gain">result in dB, [-60, 12]</param>
        /// <returns>status - 0 on success</returns>
        public static Int32 GetGain(int channel, ref float gain)
        {
            string name = "Strip[" + channel + "].gain";
            return GetParameterFloat(name, ref gain);
        }

    }
}

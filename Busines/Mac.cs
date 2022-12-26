using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace MacIdChangeGPT.Busines
{
    public static class Mac
    {
        //        private const string baseReg =
        //@"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\";

        private const string baseReg =
@"Bilgisayar\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\0011\Ndi\Params\NetworkAddress\"; // 

        public static string ReadMAC()
        {
            RegistryKey rkey;
            string MAC;
            rkey = Registry.LocalMachine.OpenSubKey("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\0011\\", true); //--->this is the string to change // 9, 11, 17
            //rkey = Registry.LocalMachine.OpenSubKey(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\00017\Ndi\params\NetworkAddress\", true); //--->this is the string to change // 0009, 0011, 0017
            MAC = (string)rkey.GetValue("NetworkAddress");
            rkey.Close();
            return MAC;
        }

        public static bool SetMAC(string newmac)
        {
            bool ret = false;

            RegistryKey rk = Registry.LocalMachine;
            string registryValue;
            string keyValue = "NetworkAddress";
            // Bilgisayar\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\0011
            string keyPath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\0011";
            RegistryKey subkey = rk.OpenSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
            var rgstryValueObj = subkey.GetValue(keyValue);
            if (rgstryValueObj != null)
                registryValue = rgstryValueObj.ToString();

            //using (RegistryKey bkey = GetBaseKey())
            using (RegistryKey key = rk.OpenSubKey(keyPath, true))// + nicid // + RegistryView.Default
            {
                if (key != null)
                {
                    key.SetValue(keyValue, newmac, RegistryValueKind.String);
                    rk.SetValue("Enable", 0);
                    rk.SetValue("Enable", 1);
                    ret = true;


                    //ManagementObjectSearcher mos = new ManagementObjectSearcher(
                    //    new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE Index = " + nicid));

                    //foreach (var o in mos.Get().OfType<ManagementObject>())
                    //{
                    //    o.InvokeMethod("Disable", null);
                    //    o.InvokeMethod("Enable", null);
                    //    ret = true;
                    //}
                }
            }

            return ret;
        }

        public static IEnumerable<string> GetNicIds()
        {
            using (RegistryKey bkey = GetBaseKey())
            using (RegistryKey key = bkey.OpenSubKey(baseReg))
            {
                if (key != null)
                {
                    foreach (string name in key.GetSubKeyNames().Where(n => n != "Properties"))
                    {
                        using (RegistryKey sub = key.OpenSubKey(name))
                        {
                            if (sub != null)
                            {
                                object busType = sub.GetValue("BusType");
                                string busStr = busType != null ? busType.ToString() : string.Empty;
                                if (busStr != string.Empty)
                                {
                                    yield return name;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static RegistryKey GetBaseKey()
        {
            return RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine,
                InternalCheckIsWow64() ? RegistryView.Registry64 : RegistryView.Registry32);
        }
        static bool is64BitProcess = (IntPtr.Size == 8);
        static bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        public static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }


    }

}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using CP210x_Managed.Properties;

#if WIN32
#error You should to change silabs dll for the other platforms in Resources.resx. In other case an System.BadImageFormatException error while be rise
#warning Don't forget to change "if" prepross statement by WIN64 or WIN32 to remember to change it when the platform is changed.
#endif


namespace CP210x_Managed
{
    public static class CP210x
    {
        public static void LoadDll()
        {
            EmbeddedDllClass.ExtractEmbeddedDlls("CP210xRuntime.dll", Resources.CP210xRuntime);
            EmbeddedDllClass.ExtractEmbeddedDlls("CP210xManufacturing.dll", Resources.CP210xManufacturing);
        }

        public enum STATUS
        {
            SUCCESS = 0x00,
            DEVICES_NOT_FOUND = 0XFF,
            INVALID_HANDLE = 0X01,
            INVALID_PARAMETER = 0x02,
            DEVICE_IO_FAILED = 0x03,
            FUNCTION_NOT_SUPPORTED = 0x04,
            GLOBAL_DATA_ERROR = 0x05,
            FILE_ERROR = 0x06,
            COMMAND_FAILED = 0x08,
            INVALID_ACCESS_TYPE = 0x09
        };

        public enum DeviceVersionCode
        {
            CP2101_VERSION = 0x01,
            CP2102_VERSION = 0x02,
            CP2103_VERSION = 0x03,
            CP2104_VERSION = 0x04,
            CP2105_VERSION = 0x05,
            CP2108_VERSION = 0x08,
            CP2109_VERSION = 0x09,
            CP2102N_QFN28_VERSION = 0x20,
            CP2102N_QFN24_VERSION = 0x21,
            CP2102N_QFN20_VERSION = 0x22
        }

        [DllImport("CP210xManufacturing.dll")]
        private static extern STATUS CP210x_GetNumDevices(out Int32 numOfDevices);
        public static STATUS GetNumDevices(out Int32 numOfDevices)
        {
            return CP210x_GetNumDevices(out numOfDevices);
        }

        [DllImport("CP210xManufacturing.dll")]
        private static extern STATUS CP210x_Open(Int32 deviceNum, ref IntPtr handle);
        public static STATUS Open(Int32 deviceNum, ref IntPtr handle)
        {
            return CP210x_Open(deviceNum, ref handle);
        }

        [DllImport("CP210xManufacturing.dll")]
        private static extern STATUS CP210x_Close(IntPtr handle);
        public static STATUS Close(IntPtr handle)
        {
            return CP210x_Close(handle);
        }

        [DllImport("CP210xManufacturing.dll")]
        private static unsafe extern STATUS CP210x_GetDeviceVid(IntPtr handle, byte* lpwVid);
        public static unsafe STATUS GetDeviceVid(IntPtr handle, out UInt32 lpwVid)
        {
            byte[] arrayChaine = new byte[4];
            STATUS ret = 0;

            fixed (byte* ptrVid = new byte[4])
            {
                ret = CP210x_GetDeviceVid(handle, ptrVid);
                Marshal.Copy((IntPtr)ptrVid, arrayChaine, 0, 4);
                lpwVid = BitConverter.ToUInt32(arrayChaine, 0);
            }
            return ret;
        }

        [DllImport("CP210xManufacturing.dll")]
        private static unsafe extern STATUS CP210x_GetDevicePid(IntPtr handle, byte* lpwPid);
        public static unsafe STATUS GetDevicePid(IntPtr handle, out UInt32 lpwPid)
        {
            byte[] arrayChaine = new byte[4];
            STATUS ret = 0;

            fixed (byte* ptrPid = new byte[4])
            {
                ret = CP210x_GetDevicePid(handle, ptrPid);
                Marshal.Copy((IntPtr)ptrPid, arrayChaine, 0, 4);
                lpwPid = BitConverter.ToUInt32(arrayChaine, 0);
            }
            return ret;
        }

        [DllImport("CP210xManufacturing.dll")]
        private static unsafe extern STATUS CP210x_GetDeviceInterfaceString(IntPtr handle, byte bInterfaceNumber, byte* lpProductString, out byte lpbProductStringLengthInBytes, bool bConvertToASCII);

        [DllImport("CP210xRuntime.dll")]
        private static unsafe extern STATUS CP210xRT_GetDeviceInterfaceString(IntPtr handle, byte* lpProductString, out byte lpbProductStringLengthInBytes, bool bConvertToASCII);
        public static unsafe STATUS GetDeviceInterfaceString(IntPtr handle, out string[] lpInterfaceString, bool bConvertToASCII = true)
        {
            byte lpbProductStringLengthInBytes = 0;
            byte[] arrayChaine;
            STATUS ret = 0;
            DeviceVersionCode partNumber;
            lpInterfaceString = null;
            int numberOfInterface = 0;

            ret = GetPartNumber(handle, out partNumber);
            if (ret != STATUS.SUCCESS) return ret;

            fixed (byte* chaine = new byte[200])
            {
                switch (partNumber)
                {
                    case DeviceVersionCode.CP2101_VERSION:
                    case DeviceVersionCode.CP2102_VERSION:
                    case DeviceVersionCode.CP2103_VERSION:
                    case DeviceVersionCode.CP2104_VERSION: numberOfInterface = 1; break;
                    case DeviceVersionCode.CP2105_VERSION: numberOfInterface = 2; break;
                    default: return STATUS.FUNCTION_NOT_SUPPORTED;
                }

                lpInterfaceString = new string[numberOfInterface];
                for (byte i = 0; i < numberOfInterface; i++)
                {
                    if (numberOfInterface == 1) ret = CP210xRT_GetDeviceInterfaceString(handle, chaine, out lpbProductStringLengthInBytes, bConvertToASCII);
                    else ret = CP210x_GetDeviceInterfaceString(handle, i, chaine, out lpbProductStringLengthInBytes, bConvertToASCII);

                    if (ret != STATUS.SUCCESS) return ret;

                    arrayChaine = new byte[lpbProductStringLengthInBytes];
                    Marshal.Copy((IntPtr)chaine, arrayChaine, 0, lpbProductStringLengthInBytes);
                    lpInterfaceString[i] = Encoding.Default.GetString(arrayChaine);
                }
            }
            return ret;
        }

        [DllImport("CP210xRuntime.dll")]
        private static extern STATUS CP210xRT_GetPartNumber(IntPtr handle, Byte[] lpbPartNum);
        public static STATUS GetPartNumber(IntPtr handle, out DeviceVersionCode partNumber)
        {
            byte[] lpbPartNum = new byte[1];
            STATUS ret = 0;
            ret = CP210xRT_GetPartNumber(handle, lpbPartNum);
            partNumber = (DeviceVersionCode)(lpbPartNum[0]);
            return ret;
        }

        [DllImport("CP210xRuntime.dll")]
        private static extern STATUS CP210xRT_WriteLatch(IntPtr handle, UInt16 mask, UInt16 latch);
        public static STATUS WriteLatch(IntPtr handle, UInt16 mask, UInt16 latch)
        {
            return CP210xRT_WriteLatch(handle, mask, latch);
        }

        [DllImport("CP210xRuntime.dll")]
        private static extern STATUS CP210xRT_ReadLatch(IntPtr handle, UInt16[] lpLatch);
        public static STATUS ReadLatch(IntPtr handle, UInt16[] lpLatch)
        {
            return CP210xRT_ReadLatch(handle, lpLatch);
        }

        [DllImport("CP210xRuntime.dll")]
        private static unsafe extern STATUS CP210xRT_GetDeviceProductString(IntPtr handle, byte* lpProductString, ref byte lpbProductStringLengthInBytes, bool bConvertToASCII);
        public static unsafe STATUS GetDeviceProductString(IntPtr handle, out string lpProductString, bool bConvertToASCII = true)
        {
            byte lpbProductStringLengthInBytes = 0;
            byte[] arrayChaine;
            STATUS ret = 0;

            fixed (byte* chaine = new byte[256])
            {
                ret = CP210xRT_GetDeviceProductString(handle, chaine, ref lpbProductStringLengthInBytes, bConvertToASCII);
                arrayChaine = new byte[lpbProductStringLengthInBytes];
                Marshal.Copy((IntPtr)chaine, arrayChaine, 0, lpbProductStringLengthInBytes);
                lpProductString = Encoding.Default.GetString(arrayChaine);
            }
            return ret;
        }

        [DllImport("CP210xRuntime.dll")]
        private static unsafe extern STATUS CP210xRT_GetDeviceSerialNumber(IntPtr handle, byte* lpProductString, ref byte lpbProductStringLengthInBytes, bool bConvertToASCII);
        public static unsafe STATUS GetDeviceSerialNumber(IntPtr handle, out string lpSerialNumberString, bool bConvertToASCII = true)
        {
            byte lpbProductStringLengthInBytes = 0;
            byte[] arrayChaine;
            STATUS ret = 0;

            fixed (byte* chaine = new byte[200])
            {
                ret = CP210xRT_GetDeviceSerialNumber(handle, chaine, ref lpbProductStringLengthInBytes, bConvertToASCII);
                arrayChaine = new byte[lpbProductStringLengthInBytes];
                Marshal.Copy((IntPtr)chaine, arrayChaine, 0, lpbProductStringLengthInBytes);
                lpSerialNumberString = Encoding.Default.GetString(arrayChaine);
            }
            return ret;
        }

        public static string[] GetSerialPort(IntPtr handle)
        {
            UInt32 Vid, Pid;
            DeviceVersionCode deviceVersion;
            string serialNumber;
            STATUS ret;


            ret = GetDeviceVid(handle, out Vid);
            if (ret != STATUS.SUCCESS) return null;
            ret = GetDevicePid(handle, out Pid);
            if (ret != STATUS.SUCCESS) return null;
            ret = GetPartNumber(handle, out deviceVersion);
            if (ret != STATUS.SUCCESS) return null;
            ret = GetDeviceSerialNumber(handle, out serialNumber);
            if (ret != STATUS.SUCCESS) return null;

            return GetSerialPort(Vid, Pid, deviceVersion, serialNumber);
        }
        public static string[] GetSerialPort(UInt32 Vid, UInt32 Pid, DeviceVersionCode DeviceVersion, string SerialNumber)
        {
            string[] ret = null;
            string regString = "SYSTEM\\CurrentControlSet\\Enum\\USB\\VID_" + Vid.ToString("X4") + "&PID_" + Pid.ToString("X4") + "\\" + SerialNumber;
            string ParentIdPrefix = null;
            try
            {


                switch (DeviceVersion)
                {
                    case DeviceVersionCode.CP2101_VERSION:
                    case DeviceVersionCode.CP2102_VERSION:
                    case DeviceVersionCode.CP2103_VERSION:
                    case DeviceVersionCode.CP2104_VERSION:
                        {
                            regString += "\\Device Parameters";
                            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regString))
                            {
                                if (key == null) throw new NullReferenceException("key object is null");
                                Object o = key.GetValue("PortName");
                                ret = new string[] { o as string };
                            }
                        }
                        break;
                    case DeviceVersionCode.CP2105_VERSION:
                        {
                            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regString))
                            {
                                if (key == null) throw new NullReferenceException("key object is null");
                                Object o = key.GetValue("ParentIdPrefix");

                                if (o == null) throw new NullReferenceException("o object is null");
                                ret = new string[2];
                                ParentIdPrefix = o as string;

                                for (int i = 0; i < 2; i++)
                                {
                                    regString = "SYSTEM\\CurrentControlSet\\Enum\\USB\\VID_" + Vid.ToString("X4") + "&PID_" + Pid.ToString("X4") + "&MI_0" + i + "\\" + ParentIdPrefix + "&000" + i + "\\Device Parameters";
                                    using (RegistryKey key2 = Registry.LocalMachine.OpenSubKey(regString))
                                    {
                                        if (key2 == null) throw new NullReferenceException("key object is null");
                                        Object o2 = key2.GetValue("PortName");
                                        if (o2 == null) throw new NullReferenceException("o object is null");
                                        ret[i] = o2 as string;
                                    }
                                }
                            }
                            break;
                        }
                    default: throw new Exception("Not implemented");
                }

            }
            catch (Exception ex)
            {
                ret = null;
            }
            return ret;
        }
    }
}

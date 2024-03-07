using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CP210x_Managed;

namespace TestDll
{
    class Program
    {
        static void Main(string[] args)
        {
            CP210x.LoadDll(); //Need to load embeded silabs unmanaged DLL from Managed dll

            Int32 nbDevice = 0;
            CP210x.DeviceVersionCode deviceVersion; ;
            string chaine = "";
            string[] chaineArray;
            int nbDeviceLoop = 0;
            UInt32 tmpUint32 = 0;
            string[] serialPort = null;

            CP210x.STATUS retValue;
            retValue = CP210x.GetNumDevices(out nbDevice);
            WriteLine("Number of device : " + nbDevice);
            WriteLine("Value of return : " + retValue);
            while (nbDeviceLoop < nbDevice)
            {
                WriteLine("Device loop " + nbDeviceLoop);
                IntPtr handle = IntPtr.Zero;
                retValue = CP210x.Open(nbDeviceLoop, ref handle);
                WriteLine("Value of return : " + retValue);

                retValue = CP210x.GetPartNumber(handle, out deviceVersion);
                WriteLine("Value of return : " + retValue + " and " + deviceVersion);

                retValue = CP210x.GetDeviceProductString(handle, out chaine);
                WriteLine("Value of return : " + retValue + " and " + chaine);

                retValue = CP210x.GetDeviceSerialNumber(handle, out chaine);
                WriteLine("Value of return : " + retValue + " and " + chaine);

                retValue = CP210x.GetDeviceInterfaceString(handle, out chaineArray);
                WriteLine("Value of return : " + retValue);
                foreach (string s in chaineArray) WriteLine("Interface name : " + s);

                retValue = CP210x.GetDeviceVid(handle, out tmpUint32);
                WriteLine("Value of return : " + retValue + " and 0x" + tmpUint32.ToString("X4"));

                retValue = CP210x.GetDevicePid(handle, out tmpUint32);
                WriteLine("Value of return : " + retValue + " and 0x" + tmpUint32.ToString("X4"));

                serialPort = CP210x.GetSerialPort(handle);
                foreach (string s in serialPort) WriteLine("Serial port : " + s);

                retValue = CP210x.Close(handle);
                WriteLine("Value of return : " + retValue);
                WriteLine("\r\n");
                nbDeviceLoop++;
            }
            
            Console.ReadKey();
        }

        static void WriteLine(string stringToWrite)
        {
            Console.WriteLine(stringToWrite);
            Debug.WriteLine(stringToWrite);
        }
    }
}

using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HIDList
{

    class Program
    {


        static void Main(string[] args)
        {
            // Initialize DirectInput
            var directInput = new DirectInput();


            Console.WriteLine("ProductGuid and InstanceGuid are accepted.\nProductGuid is consistent over all users but InstanceGuid allows mutliple devices of same type.\n\n");
            Console.WriteLine("Nr.\t" + "ProductName".PadRight(28, ' ') + "   ProductGuid                            InstanceGuid");


            IList<DeviceInstance> deviceList = directInput.GetDevices();
            deviceList = deviceList.OrderBy(o => o.ProductName).ToList();


            int i = 1;
            foreach (var device in deviceList)
            {
                Console.WriteLine(i++ + ".\t" + device.ProductName.PadRight(28, ' ') + " - " + device.ProductGuid + " - " + device.InstanceGuid);
            }


            Console.ReadLine();
        }
    }
}

using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace HIDList
{

    class Program
    {


        static void Main(string[] args)
        {

            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;



            int i = 1;
            foreach (var deviceInstance in directInput.GetDevices())
            {
                Console.WriteLine(i++ + ". " + deviceInstance.ProductName + " - " + deviceInstance.ProductGuid);
                
            }
            
        }
    }
}

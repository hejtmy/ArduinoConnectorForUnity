using System;
using System.IO.Ports;
using System.Timers;
using ArduinoConnector;

namespace ConsoleApplication1
{
    class Program
    {
        private static SerialPort port;

        static void Main(string[] args)
        {
            var arduino = Setup2();
            Timer myTimer = new Timer();
            myTimer.Elapsed += delegate { Blink(arduino); };
            myTimer.Interval = 1000;
            myTimer.Enabled = true;
            while (true)
            { 
            }
        }

        private static void Blink(Arduino arduino)
        {
            arduino.Blink();
        }

        private static void Setup()
        {
            port = new SerialPort("COM3", 9600);
            port.DtrEnable = true;
            port.RtsEnable = false;
            if (!port.IsOpen)
            {
                try
                {
                    port.Open();
                    port.WriteLine("WHO!");
                }
                catch
                {
                    Console.WriteLine("There was an error. Please make sure that the correct port was selected, and the device, plugged in.");
                }
            }
        }

        private static Arduino Setup2()
        {
            var arduino = new Arduino(ArduinoType.Generic);
            var connected = arduino.Connect();
            if(connected) Console.WriteLine("Connected");
            return arduino;
        }
    }
}

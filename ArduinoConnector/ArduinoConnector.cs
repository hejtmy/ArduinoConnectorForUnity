using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace ArduinoConnector
{
    public enum ArduinoType {Leonardo, Uno, Generic}
    public enum ArduinoEvent {DONE, RED, GREEN, YELLOW, BLUE}

    public class Arduino
    {
        public delegate void SerialInputHandler(ArduinoEvent arduinoEvent);
        public event SerialInputHandler DataIncomming;

        public Dictionary<string, ArduinoEvent> ArduinoEventDictionary = new Dictionary<string, ArduinoEvent>
        {
            {"BLUE",ArduinoEvent.BLUE},
            { "GREEN",ArduinoEvent.GREEN},
            {"RED", ArduinoEvent.RED},
            {"YELLOW", ArduinoEvent.YELLOW},
            {"DONE", ArduinoEvent.DONE}
        };
        private SerialPort _port;
        private string _comPort;

        private readonly ArduinoType _arduinoType;
        public string ComPort => _comPort;
        public Arduino(ArduinoType type)
        {
            _arduinoType = type;
        }
        #region PUBLIC API
        /// <summary>
        /// Loops through open serial ports and tries to connect to arduino sketch
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            if (IsOpen()) return true;
            _port = TryPorts();
            if (IsOpen())
            {
                _comPort = _port.PortName;
                _port.DataReceived += SendIncommingData;
            }
            return IsOpen();
        }
        public string ReadMessage()
        {
            if (IsOpen()) return _port.ReadLine();
            return null;
        }
        public bool IsOpen()
        {
            if (_port != null && _port.IsOpen) return true;
            return false;
        }
        /// <summary>
        /// Closes the port while maintaining class settings
        /// </summary>
        public void Disconnect()
        {
            if (IsOpen())
            {
                _port.DataReceived -= SendIncommingData;
                SendMessage("DISCONNECT");
                _port.Close();
            }   
        }
        public SerialPort GetSerialPort()
        {
            return _port;
        }
        /// <summary>
        /// Clears the connection and possible clutter and bad settings that might hang from previous connections
        /// </summary>
        public void Reset()
        {
            ResetArduinoConnection();
            Disconnect();
        }
        public void Blink()
        {
            SendMessage("BLINK");
        }
        public void SendPuls(bool bo)
        {
            if (IsOpen())
            {
                switch (bo)
                {
                    case true:
                        SendMessage("PULSE+");
                        break;
                    case false:
                        SendMessage("PULSE-!");
                        break;
                }
            }
        }
        public void SendMessage(string message)
        {
            if (IsOpen()) SendMessage(message, _port);
        }
        #endregion
        #region PRIVATE
        #region Establishing connection
        /// <summary>
        /// Loops through all open ports and tries to connect and get a response.
        /// </summary>
        /// <returns>Connection successful</returns>
        private SerialPort TryPorts()
        {
            if (IsOpen()) Disconnect();
            SerialPort port;
            //firstly triels the last port it was connected to 
            if (!String.IsNullOrEmpty(_comPort))
            {
                port = SetupConnection(_comPort);
                if (TryArduinoConnect(port)) return port;
            }
            //iterrates through all ports available and tries to connect
            foreach (string portName in SerialPort.GetPortNames())
            {
                var comPort = portName;
                port = SetupConnection(comPort);
                if (TryArduinoConnect(port)) return port;
                Disconnect();
            }
            return null;
        }
        //sends message to arduino and listens for arduino messaging 
        bool TryArduinoConnect(SerialPort port)
        {
            try
            {
                port.Open();
                if (ListenForConnectionMessage(port)) return true;
            }
            catch
            {
                Console.WriteLine("There was an error. Please make sure that the correct port was selected and the device, plugged in.");
            }
            return false;
        }
        /// <summary>
        /// Opens a specific connection - 
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        private SerialPort SetupConnection(string portName)
        {
            SerialPort port = new SerialPort(portName, 9600);
            if (_arduinoType == ArduinoType.Leonardo)
            {
                port.DtrEnable = true;
                port.RtsEnable = true;
            }
            return port;
        }
        /// <summary>
        /// Sends "WHO" to arduino and listens for answer "ARDUNIO". The timeout is set to be 50ms.
        /// </summary>
        /// <returns>true = "ARDUINO" retured</returns>
        /// <returns>false = something else or nothing retured</returns>
        private bool ListenForConnectionMessage(SerialPort port)
        {
            string info;
            port.WriteLine("WHO!");
            try
            {
                port.ReadTimeout = 50;
                info = port.ReadLine();
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Message was sent but no response came back. ");
                return false;
            }
            if (info.Contains("ARDUINO"))
            {
                SendDone(port);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Ends the arduino loop waiting for connection confirmation
        /// </summary>
        private void SendDone(SerialPort port)
        {
            port.WriteLine("DONE!");
        }
        /// <summary>
        /// Resets the arduino settings by sending the reset command. If the connector is connected it also disconnects from the board
        /// </summary>
        private void ResetArduinoConnection()
        {
            if (IsOpen())
            {
                SendMessage("RESTART");
                return;
            }
            SerialPort port;
            if (!String.IsNullOrEmpty(_comPort))
            {
                port = SetupConnection(_comPort);
                if (port.IsOpen)
                {
                    SendMessage("RESTART", port);
                    return;
                }
            }
            //otherwise we loop through all the way - will redo in the future.
            foreach (string portName in SerialPort.GetPortNames())
            {
                var comPort = portName;
                port = SetupConnection(comPort);
                if (port.IsOpen) SendMessage("RESTART", port);
            }
        }
        #endregion
        #region Event handling
        private void SendIncommingData(object sender, SerialDataReceivedEventArgs e)
        {
            if (DataIncomming != null)
            {
                SerialPort sp = (SerialPort)sender;
                string indata = sp.ReadLine();
                //clear input from arduino is "DONE\r\n" - serial has default setting newline character to \n 
                //It can be either changed to \r\n in serial setup or we just cheat it like this
                var indataCleared = indata.Replace("\r","");
                ArduinoEvent arduinoEvent;
                if (ArduinoEventDictionary.TryGetValue(indataCleared, out arduinoEvent))
                {
                    // Key was in dictionary; "value" contains corresponding value
                    DataIncomming(arduinoEvent);
                }
                else
                {
                    // Key wasn't in dictionary; "value" is now 0
                }
            }
        }
        #endregion
        private void SendMessage(string message, SerialPort port)
        {
            message = message + "!";
            port.WriteLine(message);
        }
        #endregion
    }
}

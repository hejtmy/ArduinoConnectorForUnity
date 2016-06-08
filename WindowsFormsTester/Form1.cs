using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArduinoConnector;
namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Arduino arduino = new Arduino(ArduinoType.Leonardo);
        private Stopwatch stopwatch;

        // This BackgroundWorker is used to demonstrate the 
        // preferred way of performing asynchronous operations.
        private BackgroundWorker backgroundWorker1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void Connect()
        {
            arduino.Disconnect();
            if (arduino.Connect())
            {
                label2.Text = "YES";
                label1.Text = arduino.ComPort;
                arduino.DataIncomming += PrintLine;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label2.Text = "NO";
            Connect();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label2.Text = "NO";
            arduino.DataIncomming -= PrintLine;
            arduino.Reset();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StartStopwatch();
            arduino.Blink();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StartStopwatch();
            arduino.SendPuls(true);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            StartStopwatch();
            arduino.SendPuls(false);
        }

        private void PrintLine(ArduinoEvent arduinoEvent)
        {
            Console.Write(arduinoEvent.ToString());
            label8.Invoke((MethodInvoker)delegate {
                label8.Text = arduinoEvent.ToString();
            });
            if (stopwatch == null) return;
            if (!stopwatch.IsRunning) return;
            stopwatch.Stop();
            label7.Invoke((MethodInvoker)delegate {
                label7.Text = stopwatch.ElapsedMilliseconds.ToString();
            });
        }
        private void StartStopwatch()
        {
            if (stopwatch == null) stopwatch = Stopwatch.StartNew();
            if (stopwatch.IsRunning) stopwatch.Stop();
            stopwatch = Stopwatch.StartNew();
        }
    }
}

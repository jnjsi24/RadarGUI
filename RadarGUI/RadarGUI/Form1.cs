using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace RadarGUI
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;
        private Timer timer;
        private Bitmap radarBitmap;
        private Graphics radarGraphics;
        private Pen radarPen;
        private Pen motionPen;
        private Pen gridPen;
        private Brush textBrush;
        private int angle;
        private float prevDistance;
        private float distance;
        private const float motionThreshold = 10.0f; // Adjust as needed for sensitivity

        private List<ScanPoint> scanPoints;
        private const int MaxPoints = 100;

        public Form1()
        {
            InitializeComponent();

            // Initialize the serial port
            serialPort = new SerialPort("COM3", 9600);
            serialPort.DataReceived += SerialPort_DataReceived;

            // Initialize the radar bitmap and graphics
            radarBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            radarGraphics = Graphics.FromImage(radarBitmap);
            radarPen = new Pen(Color.Lime, 2);
            motionPen = new Pen(Color.Red, 2);
            gridPen = new Pen(Color.DarkGreen, 1);
            textBrush = new SolidBrush(Color.Lime);

            // Initialize the timer
            timer = new Timer();
            timer.Interval = 100; // Update every 100 ms
            timer.Tick += Timer_Tick;

            // Initialize scan points list
            scanPoints = new List<ScanPoint>();

            // Set up PictureBox properties
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.BackColor = Color.Black;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Additional setup can go here if needed
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (!serialPort.IsOpen)
            {
                serialPort.Open();
                timer.Start();
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine();
                string[] values = data.Split(',');

                if (values.Length == 3)
                {
                    int newAngle = int.Parse(values[0]);
                    float newPrevDistance = float.Parse(values[1]);
                    float newDistance = float.Parse(values[2]);

                    // Update angle and distance safely
                    this.Invoke((MethodInvoker)delegate
                    {
                        angle = newAngle;
                        prevDistance = newPrevDistance;
                        distance = newDistance;

                        // Check if a full rotation is completed (assuming angle is 0 to 180)
                        if (angle == 0)
                        {
                            scanPoints.Clear(); // Clear scan points at the start of a new rotation
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., logging)
                Console.WriteLine("Exception in SerialPort_DataReceived: " + ex.Message);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            radarGraphics.Clear(Color.Black);

            // Draw grid and directions
            DrawGrid();
            DrawDirections();

            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;

            float radians = angle * (float)Math.PI / 180;
            float x = centerX + distance * (float)Math.Cos(radians);
            float y = centerY - distance * (float)Math.Sin(radians);

            // Add the current scan point to the list
            scanPoints.Add(new ScanPoint { X = x, Y = y, Distance = distance, Alpha = 255 });

            // Remove old points if the list is too long
            if (scanPoints.Count > MaxPoints)
            {
                scanPoints.RemoveAt(0);
            }

            // Draw scan points with fading effect
            foreach (var point in scanPoints)
            {
                var fadingPen = new Pen(Color.FromArgb(point.Alpha, radarPen.Color), radarPen.Width);
                radarGraphics.DrawLine(fadingPen, centerX, centerY, point.X, point.Y);
                point.Alpha = Math.Max(0, point.Alpha - 5); // Fade out gradually
            }

            // Draw the current scan line
            radarGraphics.DrawLine(radarPen, centerX, centerY, x, y);

            // Determine the direction of the motion
            string direction = "";

            if (y < centerY - (pictureBox1.Height / 4))
            {
                direction += "North";
            }
            else if (y > centerY + (pictureBox1.Height / 4))
            {
                direction += "South";
            }

            if (x < centerX - (pictureBox1.Width / 4))
            {
                direction += "West";
            }
            else if (x > centerX + (pictureBox1.Width / 4))
            {
                direction += "East";
            }

            // Determine the angle of the motion
            string angleDirection = "";

            if (angle >= 0 && angle <= 45 || angle > 315 && angle <= 360)
            {
                angleDirection += "North";
            }
            else if (angle > 45 && angle <= 135)
            {
                angleDirection += "East";
            }
            else if (angle > 135 && angle <= 225)
            {
                angleDirection += "South";
            }
            else if (angle > 225 && angle <= 315)
            {
                angleDirection += "West";
            }

            // Check for motion
            if (Math.Abs(distance - prevDistance) > motionThreshold)
            {
                // Draw motion detection sign
                radarGraphics.DrawLine(motionPen, centerX, centerY, x, y);
                radarGraphics.DrawString("Motion Detected", new Font("Arial", 12, FontStyle.Bold), Brushes.Red, new PointF(centerX - 50, centerY - 10));

                // Update the text box with the coordinates, direction, and angle direction of the detected motion
                if (!string.IsNullOrEmpty(direction) && !string.IsNullOrEmpty(angleDirection))
                {
                    location.Text = $"({x}, {y}) - {direction} {angleDirection}";
                }
                else
                {
                    location.Text = $"({x}, {y}) - {direction} {angleDirection}";
                }
            }
            else
            {
                // Clear the text box if no motion detected
                location.Text = "";
            }

            // Update the PictureBox image safely
            pictureBox1.Image = radarBitmap;
        }




        private void DrawGrid()
        {
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;
            int numCircles = 5;
            int numLines = 12;

            // Draw concentric circles
            for (int i = 1; i <= numCircles; i++)
            {
                float radius = i * (pictureBox1.Width / 2 / numCircles);
                radarGraphics.DrawEllipse(gridPen, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }

            // Draw radial lines
            for (int i = 0; i < numLines; i++)
            {
                float radians = i * (2 * (float)Math.PI / numLines);
                float x = centerX + (centerX * (float)Math.Cos(radians));
                float y = centerY - (centerY * (float)Math.Sin(radians));
                radarGraphics.DrawLine(gridPen, centerX, centerY, x, y);
            }
        }

        private void DrawDirections()
        {
            float centerX = pictureBox1.Width / 2;
            float centerY = pictureBox1.Height / 2;
            float edgeOffset = 10;

            Font font = new Font("Arial", 10, FontStyle.Bold);

            // North
            radarGraphics.DrawString("N", font, textBrush, centerX - 10, edgeOffset);
            // South
            radarGraphics.DrawString("S", font, textBrush, centerX - 10, pictureBox1.Height - edgeOffset - 20);
            // East
            radarGraphics.DrawString("E", font, textBrush, pictureBox1.Width - edgeOffset - 20, centerY - 10);
            // West
            radarGraphics.DrawString("W", font, textBrush, edgeOffset, centerY - 10);
        }

        // ScanPoint class to store scan information
        private class ScanPoint
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Distance { get; set; }
            public int Alpha { get; set; }
        }

        private void location_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
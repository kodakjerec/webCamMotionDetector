using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Vision.Motion;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AforgeCam
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection VideoCaptureDevices;
        private VideoCaptureDevice FinalVideo;
        private MotionDetector detector;
        private Rectangle[] zonesFrame, zonesPaint;

        // Importante        //
        // Installare AForge //

        public Form1() // init
        {
            InitializeComponent();
            {
                VideoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                foreach (FilterInfo VideoCaptureDevice in VideoCaptureDevices)
                {
                    comboBox1.Items.Add(VideoCaptureDevice.Name);
                }
                comboBox1.SelectedIndex = 0;

                // resize image
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

                // create motion detector
                detector = new MotionDetector(
                    new SimpleBackgroundModelingDetector(),
                    new MotionAreaHighlighting());

            }
        }

        /// <summary>
        /// 開起鏡頭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            FinalVideo = new VideoCaptureDevice(VideoCaptureDevices[comboBox1.SelectedIndex].MonikerString);
            FinalVideo.VideoResolution = FinalVideo.VideoCapabilities[0];
            FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
            FinalVideo.Start();
            setDetectZone();
        }

        /// <summary>
        /// 關閉鏡頭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Image oldImage = pictureBox1.Image;
            Bitmap video = (Bitmap)eventArgs.Frame.Clone();

            if (detector.ProcessFrame(video) > 0.05) {
                notifyIcon1.BalloonTipText = DateTime.Now.ToString("HH:mm:ss.fff");
                notifyIcon1.ShowBalloonTip(1000);
            }

            pictureBox1.Image = video;

            if (oldImage != null) {
                oldImage.Dispose();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (FinalVideo != null)
            {
                FinalVideo.SignalToStop();
                FinalVideo.WaitForStop();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FinalVideo != null)
            {
                FinalVideo.SignalToStop();
                FinalVideo.WaitForStop();
            }
        }

        /// <summary>
        /// 改變視窗比例, 也要改變paintSize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
            else
            {
                setDetectZone();
            }
        }

        /// <summary>
        /// new detectZone
        /// </summary>
        private void setDetectZone() {
            if (FinalVideo == null) {
                return;
            }

            Rectangle rectangle1,rectangle2,rectangle3;

            if (zonesFrame == null)
            {
                rectangle1 = new Rectangle(0, 0, FinalVideo.VideoResolution.FrameSize.Width, 280);
                rectangle2 = new Rectangle(0, 280, 350, 440);
                rectangle3 = new Rectangle(FinalVideo.VideoResolution.FrameSize.Width - 350, 280, 350, 440);
                zonesFrame = new Rectangle[] { rectangle1, rectangle2, rectangle3 };

                detector.MotionZones = zonesFrame;
            }
            else {
                rectangle1 = zonesFrame[0];
                rectangle2 = zonesFrame[1];
                rectangle3 = zonesFrame[2];
            }



            float percentWidth = (float)pictureBox1.Width / (float)FinalVideo.VideoResolution.FrameSize.Width;
            float percentHeight = (float)pictureBox1.Height / (float)FinalVideo.VideoResolution.FrameSize.Height;

            Rectangle rectanglePaint1 = new Rectangle(0, 0, FinalVideo.VideoResolution.FrameSize.Width, (int)(rectangle1.Height * percentHeight));
            Rectangle rectanglePaint2 = new Rectangle(0, (int)(rectangle2.Y * percentHeight), (int)(rectangle2.Width * percentWidth), (int)(rectangle2.Height * percentHeight));
            Rectangle rectanglePaint3 = new Rectangle((int)(rectangle3.X * percentWidth), (int)(rectangle3.Y * percentHeight), (int)(rectangle3.Width * percentWidth), (int)(rectangle3.Height * percentHeight));

            zonesPaint = new Rectangle[] { rectanglePaint1, rectanglePaint2, rectanglePaint3 };

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                //如果目前是縮小狀態，才要回覆成一般大小的視窗
                Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                //如果目前是縮小狀態，才要回覆成一般大小的視窗
                Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// 繪製方框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (zonesPaint == null)
            {
                return;
            }

            using (Pen pen = new Pen(Color.Red)) {
                for (int i = 0; i < zonesPaint.Length; i++) {
                    e.Graphics.DrawRectangle(pen, zonesPaint[i]);
                }
            }
        }
    }
}
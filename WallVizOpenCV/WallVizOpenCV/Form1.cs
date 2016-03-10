using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;
using FlyCapture2Managed;
using Emgu.CV.Cvb;
using System.Windows.Threading;

namespace WallVizOpenCV
{
    public partial class Form1 : Form
    {
        private long fps = 0;
        private int blobAreaMin = 300;
        private int blobAreaMax = 500;
        private int kernelSize = 7;
        private CvBlobDetector bDetect = new Emgu.CV.Cvb.CvBlobDetector();
        private Gray minGray = new Gray(80);
        private Gray maxGray = new Gray(255);
        private ManagedImage mImage = new ManagedImage();
        private Stopwatch stopwatch = new Stopwatch();
        private CvBlobs blobs = new CvBlobs();
        private DispatcherTimer uiTimer;
        float[] times = new float[] { 0f, 0f, 0f, 0f, 0f };

        private void filterImage(Emgu.CV.Image<Gray, Byte> img, int kernelSize, Gray minGray, Gray maxGray) {
            img._SmoothGaussian(kernelSize);
            img._ThresholdBinary(minGray, maxGray);
        }

        // Compare blobs to previous set of blobs.
        // Generate TUIO events for updates/new cursors.
        private void parseBlobs(CvBlobs blobs)
        {
            Console.WriteLine("Detected {0} blobs.", blobs.Count);
        }

        private void backgroundCapture(PointGreyCamera cam)
        {
            while (true)
            {
                stopwatch.Restart();
                cam.RetrieveBuffer(mImage);
                times[0] = stopwatch.ElapsedMilliseconds;
                unsafe
                {
                    IntPtr p = (IntPtr)mImage.data;
                    Image<Gray, Byte> orig = new Image<Gray, Byte>(1024, 1024, (int)mImage.stride, p);
                    Image<Gray, Byte> final = orig.Clone();
                    times[1] = stopwatch.ElapsedMilliseconds - times[0];
                    filterImage(final, 7, minGray, maxGray);
                    times[2] = stopwatch.ElapsedMilliseconds - times[1] - times[0];
                    bDetect.Detect(final, blobs);
                    blobs.FilterByArea(blobAreaMin, blobAreaMax);
                    parseBlobs(blobs);
                    times[3] = stopwatch.ElapsedMilliseconds - times[2] - times[1] - times[0];
                    times[4] = stopwatch.ElapsedMilliseconds;
                    imageBox1.Image = orig;
                    imageBox2.Image = final;
                    imageBox3.Image = bDetect.DrawBlobs(final, blobs, CvBlobDetector.BlobRenderType.BoundingBox, 1f);
                }
                while (stopwatch.ElapsedMilliseconds < 10) ;
                fps = 1000 / (int)times[4];
                Console.WriteLine("Buffer: {0} Convert: {1} Filters: {2} Detect: {3} Total: {4} (fps: {5})", times[0], times[1], times[2], times[3], times[4], fps);
            }
        }

        private void OnUITimerTick(object sender, EventArgs e)
        {
            msRetrieveBuffer.Text = "RetrieveBuffer: " + times[0];
            msConvertImg.Text = "Convert Img: " + times[1];
            msFilters.Text = "Filters: " + times[2];
            msBlobDetection.Text = "Blob Detection: " + times[3];
            msTotal.Text = "Total ms: " + times[4] + " (FPS: " + fps + ")";
        }

        public Form1()
        {
            InitializeComponent();
            PointGreyCamera cam = new PointGreyCamera(true);
            cam.StartCapture();
            Task.Factory.StartNew(() => backgroundCapture(cam));
            uiTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            uiTimer.Tick += new EventHandler(OnUITimerTick);
            uiTimer.Interval = TimeSpan.FromMilliseconds(1000);
            uiTimer.Start();
        }
    }
}

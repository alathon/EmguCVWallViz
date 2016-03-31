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
using System.Threading;

namespace WallVizOpenCV
{
    public partial class Form1 : Form
    {
        private long fps = 0;
        private int blobAreaMin = 50;
        private int blobAreaMax = 200;
        private int kernelSize = 7;
        private CvBlobDetector bDetect = new Emgu.CV.Cvb.CvBlobDetector();
        private Gray minGray = new Gray(80);
        private Gray maxGray = new Gray(255);
        private FilteredImage filteredImage = new FilteredImage(true, 5, 120);
        private ManagedImage mImage = new ManagedImage();
        private ManagedImage dest = new ManagedImage();
        private Stopwatch processStopwatch = new Stopwatch();
        private Stopwatch cameraStopwatch = new Stopwatch();
        private CvBlobs blobs = new CvBlobs();
        private DispatcherTimer uiTimer;
        float[] times = new float[] { 0f, 0f, 0f, 0f, 0f };
        private bool processing = false;
        private BlobTracker tracker = new BlobTracker();

        public Form1()
        {
            InitializeComponent();
            SetupImageBoxes();
            SetupUITimer();
            RunCamera();
        }

        // Debug info.
        private void parseBlobs(CvBlobs blobs)
        {
            //Console.WriteLine("Detected {0} blobs.", blobs.Count);
            foreach(CvBlob blob in blobs.Values) {
                Console.WriteLine("Blob: {0}", blob.Centroid);
            }
        }

        private void UpdateImageBoxes()
        {
            imageBox1.Image = this.filteredImage.BalanceImg;
            imageBox2.Image = this.filteredImage.CurrentImage;
            imageBox3.Image = this.filteredImage.DiffImage;
            imageBox4.Image = this.filteredImage.ResultImage; 
        }

        // Process a frame -> Detect blobs, track blobs, send off TUIO events.
        private void OnNewFrame(Image<Gray, Byte> image)
        {
            if(!processing) return;

            if (this.filteredImage.BalanceImg == null)
            {
                this.filteredImage.SetBalance(image.Clone());
            }
            processStopwatch.Restart();
            this.filteredImage.SetFrame(image);
            times[1] = processStopwatch.ElapsedMilliseconds;
            bDetect.Detect(this.filteredImage.ResultImage, blobs);
            bDetect.DrawBlobs(this.filteredImage.ResultImage, blobs, CvBlobDetector.BlobRenderType.Default, 0.75f);
            //blobs.FilterByArea(blobAreaMin, blobAreaMax);
            times[2] = processStopwatch.ElapsedMilliseconds;
            //var blobEvents = tracker.NewFrame(blobs);
            UpdateImageBoxes();
            processing = false;

            // Simulate delay.
            Thread.Sleep(7);
        }

        // Background capture where OnNewFrame() is handed off to a new Task.
        // Frames retrieved while the old frame is still processing are dropped.
        private void otherBackgroundCapture(PointGreyCamera cam)
        {
            cam.StartCapture();
            int current = 0;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int drops = 0;
            while (true)
            {
                timer.Restart();
                cam.RetrieveBuffer(mImage);
                unsafe
                {
                    IntPtr p = (IntPtr)mImage.data;
                    Image<Gray, Byte> orig = new Image<Gray, Byte>(1024, 1024, (int)mImage.stride, p);
                    
                    if (!processing)
                    {
                        processing = true;
                        Task.Factory.StartNew(() => OnNewFrame(orig.Clone()));
                        current++;
                    }
                    else
                    {
                        drops++;
                    }
                }
                times[0] = timer.ElapsedMilliseconds;
            }
        }

        // This is just here for comparison: OnNewFrame() happens in same thread, everything is sequential.
        private void seqBackgroundCapture(PointGreyCamera cam, int count)
        {
            cam.StartCapture();
            int current = 0;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (current < count)
            {
                cam.RetrieveBuffer(mImage);
                unsafe
                {
                    IntPtr p = (IntPtr)mImage.data;
                    Image<Gray, Byte> orig = new Image<Gray, Byte>(1024, 1024, (int)mImage.stride, p);
                    processing = true;
                    long before = timer.ElapsedMilliseconds;
                    OnNewFrame(orig.Clone());
                    long now = timer.ElapsedMilliseconds;
                    processing = false;
                    //Console.WriteLine("Frame processing: {0}ms", now - before);
                }
                current++;
            }
            timer.Stop();
            Console.WriteLine("Time taken by sequential capture: {0}ms ({1}ms per)", timer.ElapsedMilliseconds, timer.ElapsedMilliseconds / (float)count);
            cam.StopCapture();
        }

        private void OnUITimerTick(object sender, EventArgs e)
        {
            msRetrieveBuffer.Text = "RetrieveBuffer: " + times[0];
            msConvertImg.Text = "Filters: " + times[1];
            msFilters.Text = "BLob Detection: " + times[2]; 
            msBlobDetection.Text = "Blob Tracking: " + times[3];
            msTotal.Text = "Total ms: " + times[4] + " (FPS: " + fps + ")";

            Application.DoEvents();
        }

        private void SetupImageBoxes()
        {
            imageBox1.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
            imageBox2.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
            imageBox3.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
            imageBox4.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
        }

        private void RunCamera()
        {
            PointGreyCamera cam = new PointGreyCamera(15307454, true);
            Task.Factory.StartNew(() => otherBackgroundCapture(cam), TaskCreationOptions.LongRunning);
                //.ContinueWith((antecedent) => seqBackgroundCapture(cam, 1000));
        }

        private void SetupUITimer()
        {
            uiTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            uiTimer.Tick += new EventHandler(OnUITimerTick);
            uiTimer.Interval = TimeSpan.FromMilliseconds(100);
            uiTimer.Start();
        }


    }
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;
using FlyCapture2Managed;
using Emgu.CV.Cvb;
using System.Windows.Threading;
using WallVizOpenCV.BlobTracker;
using WallVizOpenCV.ConnexionServer;
using Connexion.Tcp;

namespace WallVizOpenCV
{
    public partial class Form1 : Form
    {
        // Blob tracking & Detection.
        private BlobTrackerImpl tracker = new BlobTrackerImpl();
        private CvBlobDetector bDetect = new Emgu.CV.Cvb.CvBlobDetector();
        private int blobAreaMin = 50;
        private int blobAreaMax = 300;
        private Gray minGray = new Gray(80);
        private Gray maxGray = new Gray(255);
        
        // Managed image(s).
        private FilteredImage filteredImage = new FilteredImage(5);
        private ManagedImage camImage = new ManagedImage();
        private ManagedImage camImageCopy = new ManagedImage();
        private Image<Gray, Byte> emguCvCamImage = new Image<Gray, byte>(1024, 1024);
        private Image<Bgr, Byte> detectedBlobsImage = new Image<Bgr, byte>(1024, 1024);

        // Timers.
        private Stopwatch processStopwatch = new Stopwatch();
        private Stopwatch cameraStopwatch = new Stopwatch();
        private int updateCounter = 0;
        private DispatcherTimer uiTimer;
        float[] times = new float[] { 0f, 0f, 0f, 0f, 0f };

        // Networking
        private Server connexionServer;

        // Other.
        private long fps = 0;
        private bool processing = false;

        public Form1()
        {
            InitializeComponent();
            SetupImageBoxes();
            SetupUITimer();
            RunCamera();
            SetupServer("127.0.0.1", 10001);
            StartServer();
        }
        
        private void StartServer()
        {
            this.connexionServer.Start();
            Console.WriteLine("WallViz Touch Server started.");
        }

        private void StopServer()
        {
            this.connexionServer.Stop();
            Console.WriteLine("WallViz Touch Server stopped.");
        }

        private void SetupServer(String ip, int port)
        {
            System.Net.IPAddress addr = System.Net.IPAddress.Parse(ip);
            this.connexionServer = new Server("WallViz Touch Server", addr, port);
        }
        
        private void SetImageBoxes()
        {
            imageBox1.Image = this.filteredImage.CurrentImage.Clone();
            imageBox2.Image = this.filteredImage.DiffImage.Clone();
            imageBox3.Image = this.filteredImage.ResultImage.Clone();
            imageBox4.Image = this.detectedBlobsImage;
        }

        // Process a frame -> Detect blobs, track blobs, send off TUIO events.
        private void OnNewFrame()
        {
            if (emguCvCamImage == null)
            {
                processing = false;
            }
            if (!processing)
            {
                return;
            }
            processStopwatch.Restart();
            this.filteredImage.SetFrame(emguCvCamImage);
                
            times[1] = processStopwatch.ElapsedMilliseconds;
            CvBlobs blobs = new CvBlobs();
            
            bDetect.Detect(this.filteredImage.ResultImage, blobs);
            blobs.FilterByArea(blobAreaMin, blobAreaMax);
            
            times[2] = processStopwatch.ElapsedMilliseconds - times[1];

            // Update display images every nth picture.
            if ((++updateCounter % 3) == 0)
            {
                detectedBlobsImage.Data = bDetect.DrawBlobs(this.filteredImage.ResultImage, blobs, CvBlobDetector.BlobRenderType.Default, 1f).Data;
                SetImageBoxes();
                updateCounter = 0;
            }
            var now = processStopwatch.ElapsedMilliseconds;
            BlobEvent[] blobEvents = tracker.NewFrame(BlobTrackerInterop.FrameFromCvBlobs(blobs));
            this.BroadcastBlobs(blobEvents);
            times[3] = processStopwatch.ElapsedMilliseconds - now;
            processing = false;
        }

        private void BroadcastBlobs(BlobEvent[] blobs)
        {
            if(blobs.Length > 0)
            {
                Connexion.Message msg = new Connexion.Message("BlobEvents");
                this.connexionServer.BroadcastMessage(TransferableArray.Encode<BlobEvent>(msg, "blobs", new List<BlobEvent>(blobs)));
            }
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
                cam.RetrieveBuffer(camImage);
                if (!processing)
                {
                    processing = true;
                    camImage.Convert(camImage.pixelFormat, camImageCopy);
                    unsafe
                    {
                        IntPtr p = (IntPtr)camImageCopy.data;
                        emguCvCamImage = new Image<Gray, Byte>(1024, 1024, (int)camImageCopy.stride, p);
                        current++;
                        Task.Factory.StartNew(() => OnNewFrame());
                    }
                }
                else
                {
                    drops++;
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
                cam.RetrieveBuffer(camImage);
                unsafe
                {
                    IntPtr p = (IntPtr)camImage.data;
                    Image<Gray, Byte> orig = new Image<Gray, Byte>(1024, 1024, (int)camImage.stride, p);
                    processing = true;
                    long before = timer.ElapsedMilliseconds;
                    OnNewFrame();
                    long now = timer.ElapsedMilliseconds;
                    processing = false;
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

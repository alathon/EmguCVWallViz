using Emgu.CV;
using Emgu.CV.Structure;
using FlyCapture2Managed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallVizOpenCV
{
    public class PointGreyCamera : IDisposable
    {
        private ManagedCamera cam;
        private ManagedPGRGuid guid;
        private ManagedBusManager busMgr;
        private bool softwareTrigger = true;
        public bool Ready { get; private set; }

        static void PrintCameraInfo(CameraInfo camInfo)
        {
            StringBuilder newStr = new StringBuilder();
            newStr.Append("\n*** CAMERA INFORMATION ***\n");
            newStr.AppendFormat("Serial number - {0}\n", camInfo.serialNumber);
            newStr.AppendFormat("Camera model - {0}\n", camInfo.modelName);
            newStr.AppendFormat("Camera vendor - {0}\n", camInfo.vendorName);
            newStr.AppendFormat("Sensor - {0}\n", camInfo.sensorInfo);
            newStr.AppendFormat("Resolution - {0}\n", camInfo.sensorResolution);

            Console.WriteLine(newStr);
        }

        void OnProcessExit(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void ConnectToCamera()
        {
            uint numCameras = busMgr.GetNumOfCameras();
            Console.WriteLine("Number of cameras: {0}", numCameras);
            guid = busMgr.GetCameraFromIndex(0);

            // Connect.
            cam.Connect(guid);
            //cam.StopCapture();
            
            // Power on the camera
            const uint k_cameraPower = 0x610;
            const uint k_powerVal = 0x80000000;
            cam.WriteRegister(k_cameraPower, k_powerVal);

            const Int32 k_millisecondsToSleep = 100;
            uint regVal = 0;

            // Wait for camera to complete power-up
            do
            {
                System.Threading.Thread.Sleep(k_millisecondsToSleep);
                regVal = cam.ReadRegister(k_cameraPower);
            } while ((regVal & k_powerVal) == 0);

            Console.WriteLine("Camera powered on.");
            // Attach dispose to software exit.
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            // Print camera info.
            CameraInfo camInfo = cam.GetCameraInfo();
            PrintCameraInfo(camInfo);
        }

        private void SetTriggerMode()
        {
            // Get current trigger settings
            TriggerMode triggerMode = cam.GetTriggerMode();

            // Set camera to trigger mode 0
            // A source of 7 means software trigger
            triggerMode.onOff = true;
            triggerMode.mode = 0;
            triggerMode.parameter = 0;

            if (softwareTrigger)
            {
                Console.WriteLine("Trigger mode: Software.");
                triggerMode.source = 7;
            }
            else
            {
                Console.WriteLine("Trigger mode: Hardware.");
                triggerMode.source = 0;
            }

            // Set the trigger mode
            cam.SetTriggerMode(triggerMode);
        }

        private void SetFormat7Settings()
        {
            Format7ImageSettings currSettings = new Format7ImageSettings();
            uint currPacketSize = 0;
            float percentage = 0f;
            cam.GetFormat7Configuration(currSettings, ref currPacketSize, ref percentage);

            
            Format7ImageSettings settings = new Format7ImageSettings();
            settings.mode = Mode.Mode0;
            settings.offsetX = 512;
            settings.offsetY = 512;
            settings.width = 1024;
            settings.height = 1024;
            settings.pixelFormat = PixelFormat.PixelFormatMono8;

            bool needRestart = true;
            try
            {
                cam.StopCapture();
            }
            catch (FC2Exception ex)
            {
                if (ex.Type == ErrorType.IsochNotStarted)
                {
                    // This means the camera was stopped and therefore we
                    // do not need to restart it
                    needRestart = false;
                }
            }

            bool supported = true;
            Format7Info info = cam.GetFormat7Info(Mode.Mode0, ref supported);

            try
            {
                cam.SetFormat7Configuration(settings, info.maxPacketSize);
            }
            catch (FC2Exception settingFormat7Exception)
            {
                Console.WriteLine(settingFormat7Exception);
                Console.WriteLine("Error setting Format 7 settings. Returning to previous settings.");
                cam.SetFormat7Configuration(currSettings, percentage);
            }

            Format7ImageSettings f7Settings = new Format7ImageSettings();
            float pSpeed = info.percentage;
            uint pSize = info.maxPacketSize;
            cam.GetFormat7Configuration(f7Settings, ref pSize, ref pSpeed);
            Console.WriteLine("Height: {0} Width: {1} OffsetX: {2} OffsetY: {3} Mode: {4}", f7Settings.height, f7Settings.width, f7Settings.offsetX, f7Settings.offsetY, f7Settings.mode);

            if (needRestart)
            {
                try
                {
                    cam.StartCapture();
                }
                catch (FC2Exception ex)
                {
                    Console.WriteLine("Error restarting camera capture: {0}", ex);
                }
            }
        }

        private void SetGrabTimeout()
        {
            // Get the camera configuration
            FC2Config config = cam.GetConfiguration();
            // Set the grab timeout to 5 seconds
            config.grabTimeout = 5000;
            // Set the camera configuration
            cam.SetConfiguration(config);
        }

        private void Configure()
        {
            SetTriggerMode();
            SetFormat7Settings();
            SetGrabTimeout();
            SetProperties();
            PollForTriggerReady();
        }

        private void SetProperties()
        {
            CameraProperty exposure = cam.GetProperty(PropertyType.AutoExposure);
            exposure.autoManualMode = false;
            exposure.absControl = true;
            exposure.absValue = -2f;
            try
            {
                cam.SetProperty(exposure);
            }
            catch (FC2Exception ex)
            {
                Console.WriteLine("Failed to write " + exposure.type + " to camera. Error:" + ex.Message);
            }

            CameraProperty brightness = cam.GetProperty(PropertyType.Brightness);
            brightness.autoManualMode = false;
            brightness.absControl = true;
            brightness.absValue = 5.0f;
            try
            {
                cam.SetProperty(brightness);
            }
            catch (FC2Exception ex)
            {
                Console.WriteLine("Failed to write " + brightness.type + " to camera. Error:" + ex.Message);
            }

            CameraProperty sharpness = cam.GetProperty(PropertyType.Sharpness);
            sharpness.autoManualMode = false;
            sharpness.valueA = 1024;
            try
            {
                cam.SetProperty(sharpness);
            }
            catch (FC2Exception ex)
            {
                Console.WriteLine("Failed to write " + sharpness.type + " to camera. Error:" + ex.Message);
            }

            CameraProperty gamma = cam.GetProperty(PropertyType.Gamma);
            gamma.autoManualMode = false;
            gamma.absControl = true;
            gamma.absValue = 2.250f;
            try
            {
                cam.SetProperty(gamma);
            }
            catch (FC2Exception ex)
            {
                Console.WriteLine("Failed to write " + gamma.type + " to camera. Error:" + ex.Message);
            }

            //CameraProperty shutter = new CameraProperty();
            //shutter.type = PropertyType.Shutter;
            //shutter.autoManualMode = false;
            //shutter.autoManualMode = true;
            //shutter.onOff = true;
            //shutter.onePush = false;
            //shutter.absControl = true;
            ////prop.absValue = 9.926f;
            //cam.SetProperty(shutter);

            CameraProperty gain = cam.GetProperty(PropertyType.Gain);
            gain.autoManualMode = false;
            gain.absControl = true;
            gain.absValue = 0f;
            try
            {
                cam.SetProperty(gain);
            }
            catch (FC2Exception ex)
            {
                Console.WriteLine("Failed to write " + gain.type + " to camera. Error:" + ex.Message);
            }

            CameraProperty fps = cam.GetProperty(PropertyType.FrameRate);
            fps.autoManualMode = false;
            fps.absControl = true;
            fps.absValue = 100.0f;
            try
            {
                cam.SetProperty(fps);
            }
            catch (FC2Exception ex)
            {
                Console.WriteLine("Failed to write " + fps.type + " to camera. Error:" + ex.Message);
            }

            Console.WriteLine("FPS: {0}", cam.GetProperty(PropertyType.FrameRate).absValue);
            Console.WriteLine("Gain: {0}", cam.GetProperty(PropertyType.Gain).absValue);
            Console.WriteLine("Shutter: {0}", cam.GetProperty(PropertyType.Shutter).absValue);
            Console.WriteLine("Gamma: {0}", cam.GetProperty(PropertyType.Gamma).absValue);
            Console.WriteLine("Sharpness: {0}", cam.GetProperty(PropertyType.Sharpness).valueA);
            Console.WriteLine("Brightness: {0}", cam.GetProperty(PropertyType.Brightness).absValue);
            Console.WriteLine("Exposure: {0}", cam.GetProperty(PropertyType.AutoExposure).absValue);
        }
        public PointGreyCamera(bool softwareTrigger = true)
        {
            Ready = false;
            this.softwareTrigger = softwareTrigger;
            busMgr = new ManagedBusManager();
            cam = new ManagedCamera();
            ConnectToCamera();
            Configure();
            Ready = true;
        }

        bool PollForTriggerReady()
        {
            const uint k_softwareTrigger = 0x62C;

            uint regVal = 0;

            do
            {
                regVal = cam.ReadRegister(k_softwareTrigger);
            }
            while ((regVal >> 31) != 0);

            return true;
        }

        bool FireSoftwareTrigger()
        {
            const uint k_softwareTrigger = 0x62C;
            const uint k_fireVal = 0x80000000;

            cam.WriteRegister(k_softwareTrigger, k_fireVal);

            return true;
        }

        public void StartCapture()
        {
            while (!Ready) ;
            cam.StartCapture();
        }

        public void StopCapture()
        {
            cam.StopCapture();
            cam.Disconnect();
        }

        public void RetrieveBuffer(ManagedImage img)
        {
            if (softwareTrigger)
            {
                
                PollForTriggerReady();
                bool retVal = FireSoftwareTrigger();
                if (retVal != true)
                {
                    Console.WriteLine("Error firing software trigger!");
                    throw new Exception("Error firing software trigger!");
                }
            }
            cam.RetrieveBuffer(img);
        }

        public void Dispose()
        {
            cam.StopCapture();
            cam.Disconnect();
        }


        void IDisposable.Dispose()
        {
            cam.StopCapture();
            cam.Disconnect();
        }
    }
}

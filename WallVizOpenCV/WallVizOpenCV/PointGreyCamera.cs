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

        private void ConnectToCamera()
        {
            uint numCameras = busMgr.GetNumOfCameras();
            Console.WriteLine("Number of cameras: {0}", numCameras);
            guid = busMgr.GetCameraFromIndex(0);
            cam.Connect(guid);
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
                triggerMode.source = 7;
            }
            else
            {
                triggerMode.source = 0;
            }

            // Set the trigger mode
            cam.SetTriggerMode(triggerMode);
        }

        private void Configure()
        {
            //SetTriggerMode();
            //// Get the camera configuration
            //FC2Config config = cam.GetConfiguration();

            //// Set the grab timeout to 5 seconds
            //// TODO: What is grab timeout??? -- Martin
            //config.grabTimeout = 5000;
            //// Set the camera configuration
            //cam.SetConfiguration(config);

            Format7ImageSettings settings = new Format7ImageSettings();
            settings.mode = Mode.Mode0;
            settings.offsetX = 512;
            settings.offsetY = 512;
            settings.width = 1024;
            settings.height = 1024;
            settings.pixelFormat = PixelFormat.PixelFormatMono8;

            bool bah = true;
            Format7Info info = cam.GetFormat7Info(Mode.Mode0, ref bah);
            cam.SetFormat7Configuration(settings, info.maxPacketSize);

            //CameraProperty prop = new CameraProperty();
            //prop.type = PropertyType.AutoExposure;
            //prop.autoManualMode = true;
            //prop.onOff = false;
            //prop.onePush = false;
            //prop.absControl = true;
            //cam.SetProperty(prop);

            //prop.type = PropertyType.Brightness;
            //prop.autoManualMode = false;
            //prop.onOff = true;
            //prop.onePush = false;
            //prop.absControl = true;
            //prop.absValue = 5.078f;
            //cam.SetProperty(prop);

            //prop.type = PropertyType.Sharpness;
            //prop.autoManualMode = false;
            //prop.onOff = true;
            //prop.onePush = false;
            //prop.absControl = false;
            //prop.valueA = 1024;
            ////prop.absValue = 1024;
            //cam.SetProperty(prop);

            //prop.type = PropertyType.Gamma;
            //prop.autoManualMode = false;
            //prop.onOff = true;
            //prop.onePush = false;
            //prop.absControl = true;
            //prop.absValue = 1.250f;
            //cam.SetProperty(prop);

            //prop.type = PropertyType.Shutter;
            //prop.autoManualMode = false;
            //prop.onOff = true;
            //prop.onePush = false;
            //prop.absControl = true;
            //prop.absValue = 9.926f;
            //cam.SetProperty(prop);

            //prop.type = PropertyType.Gain;
            //prop.autoManualMode = true;
            //prop.onOff = true;
            //prop.onePush = false;
            //prop.absControl = false;
            ////prop.absValue = 9.926f;
            //cam.SetProperty(prop);

            //prop.type = PropertyType.FrameRate;
            //prop.autoManualMode = false;
            //prop.onOff = true;
            //prop.onePush = false;
            //prop.absControl = true;
            //prop.absValue = 100.0f;
            //cam.SetProperty(prop);
            //// TODO: Set fps, exposure, etc etc.

            CameraProperty fps = cam.GetProperty(PropertyType.FrameRate);
            Console.WriteLine("FPS: {0}", fps.absValue);
        }

        public PointGreyCamera(bool softwareTrigger = true)
        {
            this.softwareTrigger = softwareTrigger;
            busMgr = new ManagedBusManager();
            cam = new ManagedCamera();
            ConnectToCamera();
            Configure();
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
            cam.StartCapture();
        }

        public void StopCapture()
        {
            cam.StopCapture();
            cam.Disconnect();
        }

        public void RetrieveBuffer(ManagedImage img)
        {
            // Fire software trigger
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

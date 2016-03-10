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

        private void Configure()
        {
            SetTriggerMode();
            // Get the camera configuration
            FC2Config config = cam.GetConfiguration();

            // Set the grab timeout to 5 seconds
            // TODO: What is grab timeout??? -- Martin
            config.grabTimeout = 5000;
            // Set the camera configuration
            cam.SetConfiguration(config);

            Format7ImageSettings settings = new Format7ImageSettings();
            settings.mode = Mode.Mode0;
            settings.offsetX = 512;
            settings.offsetY = 512;
            settings.width = 1024;
            settings.height = 1024;
            settings.pixelFormat = PixelFormat.PixelFormatMono8;

            bool supported = true;
            Format7Info info = cam.GetFormat7Info(Mode.Mode0, ref supported);
            cam.SetFormat7Configuration(settings, info.maxPacketSize);

            Format7ImageSettings f7Settings = new Format7ImageSettings();
            float pSpeed = info.percentage;
            uint pSize = info.maxPacketSize;
            cam.GetFormat7Configuration(f7Settings, ref pSize, ref pSpeed);
            Console.WriteLine("Height: {0} Width: {1} OffsetX: {2} OffsetY: {3} Mode: {4}", f7Settings.height, f7Settings.width, f7Settings.offsetX, f7Settings.offsetY, f7Settings.mode);

            CameraProperty exposure = new CameraProperty();
            exposure.type = PropertyType.AutoExposure;
            exposure.autoManualMode = false;
            exposure.onOff = false;
            exposure.onePush = false;
            exposure.absControl = true;
            exposure.absValue = -2f;
            cam.SetProperty(exposure);

            CameraProperty brightness = new CameraProperty();
            brightness.type = PropertyType.Brightness;
            brightness.autoManualMode = false;
            brightness.onOff = true;
            brightness.onePush = false;
            brightness.absControl = true;
            brightness.absValue = 5.0f;
            cam.SetProperty(brightness);

            CameraProperty sharpness = new CameraProperty();
            sharpness.type = PropertyType.Sharpness;
            sharpness.autoManualMode = false;
            sharpness.onOff = true;
            sharpness.onePush = false;
            sharpness.absControl = false;
            sharpness.valueA = 1024;
            cam.SetProperty(sharpness);

            CameraProperty gamma = new CameraProperty();
            gamma.type = PropertyType.Gamma;
            gamma.autoManualMode = false;
            gamma.onOff = true;
            gamma.onePush = false;
            gamma.absControl = true;
            gamma.absValue = 2.250f;
            cam.SetProperty(gamma);

            //CameraProperty shutter = new CameraProperty();
            //shutter.type = PropertyType.Shutter;
            //shutter.autoManualMode = false;
            //shutter.autoManualMode = true;
            //shutter.onOff = true;
            //shutter.onePush = false;
            //shutter.absControl = true;
            ////prop.absValue = 9.926f;
            //cam.SetProperty(shutter);

            CameraProperty gain = new CameraProperty();
            gain.type = PropertyType.Gain;
            gain.autoManualMode = false;
            gain.onOff = true;
            gain.onePush = false;
            gain.absControl = true;
            gain.absValue = 0f;
            cam.SetProperty(gain);

            CameraProperty fps = new CameraProperty();
            fps.type = PropertyType.FrameRate;
            fps.autoManualMode = false;
            fps.onOff = true;
            fps.onePush = false;
            fps.absControl = true;
            fps.absValue = 100.0f;
            cam.SetProperty(fps);

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

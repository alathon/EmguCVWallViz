using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallVizOpenCV
{
    public class FilteredImage
    {
        public FilteredImage(bool balance, int kernelSize, int binaryThreshold, int binaryMaxValue)
        {
            this.balance = balance;
            this.kernelSize = kernelSize;
            this.minThresh = new Gray(binaryMaxValue);
            this.maxThresh = new Gray(binaryThreshold);
        }

        private bool balance;
        private int kernelSize;
        private Gray minThresh;
        private Gray maxThresh;

        public void SetLowThreshold(int v)
        {
            this.minThresh = new Gray(v);
        }

        public void SetKernelSize(int v)
        {
            this.kernelSize = v;
        }

        public Image<Gray, Byte> BalanceImg
        {
            get;
            private set;
        }
        public Image<Gray, Byte> GrayscaleImg
        {
            get;
            private set;
        }
        public Image<Gray, Byte> GaussImg
        {
            get;
            private set;
        }
        public Image<Gray, Byte> ThresholdImg
        {
            get;
            private set;
        }
        public Image<Gray, Byte> DiffImg
        {
            get;
            private set;
        }

        public void SetFrame(Image<Gray, Byte> image)
        {
            GrayscaleImg = image;
            Console.WriteLine("1");
            GrayscaleImg._SmoothGaussian(kernelSize);
            Console.WriteLine("2");
            GrayscaleImg._ThresholdBinary(minThresh, maxThresh);
            Console.WriteLine("3");

            //GaussImg = GrayscaleImg; // GrayscaleImg.SmoothGaussian(kernelSize);
            //if (BalanceImg != null && balance)
           // {
            //    DiffImg = BalanceImg.AbsDiff(GaussImg);
                //ThresholdImg = DiffImg.ThresholdAdaptive()
             //   ThresholdImg = DiffImg.ThresholdBinary(new Gray(binaryThreshold), new Gray(binaryMaxValue));
           // }
           // else
           // {
           //     ThresholdImg = GaussImg.ThresholdBinary(new Gray(binaryThreshold), new Gray(binaryMaxValue));
           // }
        }
        public void SetFrame(Mat frame)
        {
            SetFrame(frame.ToImage<Gray, Byte>());
        }

        public void SetBalanceImg(Image<Gray, Byte> image)
        {
            BalanceImg = image.SmoothGaussian(kernelSize);
        }

        public void SetBalanceImg(Mat frame)
        {
            BalanceImg = frame.ToImage<Gray, Byte>().SmoothGaussian(kernelSize);
        }


    }
}
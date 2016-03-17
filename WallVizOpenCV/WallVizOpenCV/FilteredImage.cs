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
        public FilteredImage(bool balance, int kernelSize, int binaryThreshold)
        {
            this.balance = balance;
            this.kernelSize = kernelSize;
            this.minThresh = new Gray(binaryThreshold);
            this.maxThresh = new Gray(255);
        }

        private bool balance;
        private int kernelSize;
        private Gray minThresh;
        private Gray maxThresh;

        public void SetThreshold(int v)
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
        public Image<Gray, Byte> ResultImage
        {
            get;
            private set;
        }
        public Image<Gray, Byte> CurrentImage
        {
            get;
            private set;
        }
        public Image<Gray, Byte> DiffImage
        {
            get;
            private set;
        }

        private void AdaptiveResultImage(Image<Gray, Byte> img)
        {
            ResultImage = img.ThresholdAdaptive(maxThresh, Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC, Emgu.CV.CvEnum.ThresholdType.Binary, 21, new Gray(-5));
        }

        private void BinaryThreshResultImage(Image<Gray, Byte> img)
        {
            ResultImage = img.Clone();
            ResultImage._ThresholdBinary(minThresh, maxThresh);
        }

        public void SetBalance(Image<Gray, Byte> image)
        {
            BalanceImg = image.Clone();
            BalanceImg._SmoothGaussian(kernelSize);
        }

        public void SetFrame(Image<Gray, Byte> image)
        {
            CurrentImage = image;
            CurrentImage._SmoothGaussian(kernelSize);

            if (balance && BalanceImg != null)
            {
                DiffImage = CurrentImage.AbsDiff(BalanceImg);
                // TODO: Consider equalizing the DiffImage, to stretch out the histogram and get more control over where to cut off.
                //DiffImage = DiffImage.InRange(new Gray(25), new Gray(60));
            }
            else
            {
                DiffImage = CurrentImage;
            }
            BinaryThreshResultImage(DiffImage);
        }
    }
}
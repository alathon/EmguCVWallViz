using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WallVizOpenCV
{
    public class FilteredImage
    {
        public FilteredImage(int kernelSize, int binaryThreshold)
        {
            this.kernelSize = kernelSize;
            this.minThresh = new Gray(binaryThreshold);
            this.maxThresh = new Gray(255);
            this.BalanceImg = new Image<Gray, byte>(new Size(1024, 1024));
            this.ResultImage = new Image<Gray, byte>(new Size(1024, 1024));
            this.DiffImage = new Image<Gray, byte>(new Size(1024, 1024));
            this.CurrentImage = new Image<Gray, byte>(new Size(1024, 1024));
        }

        private bool balanceSet = false;
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

        private void BinaryThreshResultImage(Image<Gray, Byte> img)
        {
            double[] t = minThresh.MCvScalar.ToArray();
            double[] m = maxThresh.MCvScalar.ToArray();
            CvInvoke.Threshold(img, ResultImage, t[0], m[0], Emgu.CV.CvEnum.ThresholdType.Binary);
        }


        public void SetBalance(Image<Gray, Byte> image)
        {
            CvInvoke.GaussianBlur(image, BalanceImg, new Size(kernelSize, kernelSize), 0, 0);
        }

        public Mat cur = new Mat(new Size(1024, 1024), Emgu.CV.CvEnum.DepthType.Cv8U, 1);
        public Mat balance = new Mat(new Size(1024, 1024), Emgu.CV.CvEnum.DepthType.Cv8U, 1);
        public Mat diff = new Mat(new Size(1024, 1024), Emgu.CV.CvEnum.DepthType.Cv8U, 1);
        public Mat res = new Mat(new Size(1024, 1024), Emgu.CV.CvEnum.DepthType.Cv8U, 1);
        public Image<Gray, Byte> resImage;
        
        public void SetFrame(Image<Gray, Byte> image)
        {
            CvInvoke.GaussianBlur(image, CurrentImage, new Size(kernelSize, kernelSize), 0, 0);

            if (!balanceSet)
            {
                balanceSet = true;
                BalanceImg = CurrentImage.Clone();
            }

            CvInvoke.AbsDiff(CurrentImage, BalanceImg, DiffImage);
            using (ScalarArray ialower = new ScalarArray(new Gray(25).MCvScalar))
            using (ScalarArray iaupper = new ScalarArray(new Gray(60).MCvScalar))
                CvInvoke.InRange(DiffImage, ialower, iaupper, DiffImage);
            // TODO: Consider equalizing the DiffImage, to stretch out the histogram and get more control over where to cut off.

            double[] t = minThresh.MCvScalar.ToArray();
            double[] m = maxThresh.MCvScalar.ToArray();
            CvInvoke.Threshold(DiffImage, ResultImage, t[0], m[0], Emgu.CV.CvEnum.ThresholdType.Binary);
        }
        /*
        public void SetFrame(Image<Gray, Byte> image)
        {
            CvInvoke.GaussianBlur(image, cur, new Size(kernelSize, kernelSize), 0, 0);

            if (!balanceSet)
            {
                balanceSet = true;
                balance = cur.Clone();
            }

            CvInvoke.AbsDiff(cur, balance, diff);
            using (ScalarArray ialower = new ScalarArray(new Gray(25).MCvScalar))
            using (ScalarArray iaupper = new ScalarArray(new Gray(60).MCvScalar))
                CvInvoke.InRange(diff, ialower, iaupper, diff);
            // TODO: Consider equalizing the DiffImage, to stretch out the histogram and get more control over where to cut off.

            double[] t = minThresh.MCvScalar.ToArray();
            double[] m = maxThresh.MCvScalar.ToArray();
            CvInvoke.Threshold(diff, res, t[0], m[0], Emgu.CV.CvEnum.ThresholdType.Binary);

            //ResultImage = res.ToImage<Gray, Byte>();
        }*/
    }
}
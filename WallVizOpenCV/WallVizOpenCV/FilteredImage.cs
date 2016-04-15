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
        public FilteredImage(int kernelSize)
        {
            this.kernelSize = kernelSize;
            this.BalanceImg = new Image<Gray, byte>(new Size(1024, 1024));
            this.ResultImage = new Image<Gray, byte>(new Size(1024, 1024));
            this.DiffImage = new Image<Gray, byte>(new Size(1024, 1024));
            this.CurrentImage = new Image<Gray, byte>(new Size(1024, 1024));
        }

        private bool balanceSet = false;
        private int kernelSize;
        
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
        
        private void applyGaussianBlur(Image<Gray, Byte> from, Image<Gray, Byte> to)
        {
            CvInvoke.GaussianBlur(from, to, new Size(kernelSize, kernelSize), 0, 0);
        }

        public void SetBalance(Image<Gray, Byte> image)
        {
            applyGaussianBlur(image, BalanceImg);
        }
        
        public void SetFrame(Image<Gray, Byte> image)
        {
            applyGaussianBlur(image, CurrentImage);

            if (!balanceSet)
            {
                // Set balance image. Only happens once.
                // Screen should be absolutely clear of any touching when it is set.
                balanceSet = true;
                BalanceImg = CurrentImage.Clone();
            }
            
            // Create diff image.
            CvInvoke.AbsDiff(CurrentImage, BalanceImg, DiffImage);
            
            // Amplify image.
            DiffImage *= 7f;

            // Find max intensity.
            double minVal = 0f;
            double maxVal = 0f;
            Point minLoc = Point.Empty;
            Point maxLoc = Point.Empty;
            CvInvoke.MinMaxLoc(DiffImage, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            // BInarize based on the most intense pixel in image, which is assumed to be fingertips or something else
            // touching the screen.
            // If it is not sensitive enough, lower minIntensity. If it is too sensitive, raise minIntensity.
            // You can also tweak the multipliers below to adjust how much pixels can deviate from the found value and
            // still be considered valid. Right now its +- 25%
            double minIntensity = 75f;
            double intensity = Math.Max(maxVal, minIntensity);
            using (ScalarArray ialower = new ScalarArray(new Gray(intensity * 0.75f).MCvScalar))
            using (ScalarArray iaupper = new ScalarArray(new Gray(intensity * 1.25f).MCvScalar))
                CvInvoke.InRange(DiffImage, ialower, iaupper, ResultImage);
        }
    }
}
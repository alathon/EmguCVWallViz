using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Cvb;

namespace WallVizOpenCV.BlobTracker
{
    static class BlobTrackerInterop
    {
        public static double[][] FrameFromCvBlobs(CvBlobs blobs)
        {
            double[][] frame = new double[blobs.Values.Count][];
            for (int i = 0; i < blobs.Values.Count; i++)
            {
                var blob = blobs.Values.ElementAt(i);
                frame[i] = new double[2] { blob.Centroid.X, blob.Centroid.Y };
            }
            return frame;
        }
    }
}

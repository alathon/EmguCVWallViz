using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WallVizOpenCV.BlobTracker
{
    public enum BlobEventType { Down, Up, Move, FilterOut }

    public class BlobEvent
    {
        public double[] Location;
        public double[] Origin;
        public string Id = "Unknown ID";
        public BlobEventType Event;
        public bool Tracked = false;
    }
}

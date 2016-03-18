using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Cvb;
using System.Drawing;

namespace WallVizOpenCV
{
    public enum BlobEventType { New,  Dead,  Move }

    public class BlobTrackerEvent
    {
        public CvBlob Blob;
        public BlobEventType Event;

        public BlobTrackerEvent(CvBlob blob, BlobEventType eventType) {
            this.Blob = blob;
            
            this.Event = eventType;
        }
    }

    public class TrackedBlob {
        public string Id;
        public PointF Centroid;
        public PointF Origin;
    }

    public class BlobTracker
    {
        public LinkedList<TrackedBlob> trackedBlobs = new LinkedList<TrackedBlob>();
        public CvBlobs LastFrame;
        public CvBlobs CurrentFrame;

        public void NewFrame(CvBlobs frame)
        {
            if (CurrentFrame != null)
            {
                LastFrame = CurrentFrame;
                CurrentFrame = frame;
            }
            else
            {
                LastFrame = CurrentFrame = frame;
            }
            track();
        }

        private void track()
        {
            if (CurrentFrame == null || LastFrame == null) return;

            double maxRadius = 0.5;
            var lastBlobs = LastFrame.Values;
            var currBlobs = CurrentFrame.Values;

            bool[] tracked = Enumerable.Repeat(false, trackedBlobs.Count).ToArray<bool>();
            var events = new LinkedList<BlobTrackerEvent>();

            // TODO: Write blob tracking algorithm, probably based on CCV's or somesuch.

            // This is O(n^2)
            for(int i = 0; i < currBlobs.Count; i++) {
                CvBlob curBlob = currBlobs.ElementAt(i);

                for(int j = 0; j < lastBlobs.Count; j++) {
                    if(tracked[j]) continue;

                    CvBlob lastBlob = lastBlobs.ElementAt(j);
                    if(Math.Sqrt(Math.Pow(curBlob.Centroid.X - lastBlob.Centroid.X, 2.0f) + 
                                 Math.Pow(curBlob.Centroid.Y - lastBlob.Centroid.Y, 2.0f)) < maxRadius) {
                        tracked[j] = true;
                        events.AddFirst(new BlobTrackerEvent(curBlob, BlobEventType.Move));
                    }
                }
            }
        }
    }
}

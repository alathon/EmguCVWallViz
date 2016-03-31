using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Cvb;
using System.Drawing;
using Accord.MachineLearning;
using Accord.MachineLearning.Structures;

namespace WallVizOpenCV
{
    public enum BlobEventType { Down,  Up,  Move, FilterOut }

    public class BlobEvent
    {
        public double[] Location;
        public double[] Origin;
        public string Id = "Unknown ID";
        public BlobEventType Event;
        public bool Tracked = false;
    }

    public class BlobTracker
    {
        public List<BlobEvent> lastBlobEvents { get; private set; }
        public List<BlobEvent> currentBlobEvents { get; private set; }
        public CvBlobs lastCvBlobs { get; private set; }
        public CvBlobs currentCvBlobs { get; private set; }

        public BlobTracker() {
            this.lastBlobEvents = new List<BlobEvent>();
            this.currentBlobEvents = new List<BlobEvent>();
        }

        public BlobEvent[] NewFrame(CvBlobs frame)
        {
            lastCvBlobs = currentCvBlobs;
            currentCvBlobs = frame;
            lastBlobEvents = new List<BlobEvent>(currentBlobEvents);
            currentBlobEvents = new List<BlobEvent>();
            return track();
        }

        private KDTree<BlobEvent> makeKDTree(List<BlobEvent> events)
        {
            KDTree<BlobEvent> tree = new KDTree<BlobEvent>(2);
            foreach (BlobEvent evt in events)
            {
                tree.Add(evt.Location, evt);
            }
            return tree;
        }

        private string generateId()
        {
            return System.Guid.NewGuid().ToString();
        }

        private BlobEvent[] track()
        {
            if (currentCvBlobs == null) return new BlobEvent[0];
               
            double maxRadius = 0.5; // max distance between two blobs for them to be considered the same.
            
            // DOWN EVENTS
            // All current blobs get set to a DOWN event for now.
            foreach (var entry in currentCvBlobs.Values)
            {
                var loc = new double[2] { entry.Centroid.X, entry.Centroid.Y };
                var blob = new BlobEvent() { Location = loc, Event = BlobEventType.Down };
                currentBlobEvents.Add(blob);
            }

            // If we only have one frame, all events are adds, and thats fine.
            if (lastCvBlobs == null) return currentBlobEvents.ToArray();

            // MOVE EVENTS
            // Find blobs within a distance of prior blobs, and consider those to be MOVE instead of DOWN.
            // Construct a 2-dimensional KD-Tree of the blob events from last frame.
            KDTree<BlobEvent> tree = makeKDTree(lastBlobEvents);

            // For each current blob, find the closest blob in the last blob events, if any.
            for (int i = 0; i < currentBlobEvents.Count; i++)
            {
                BlobEvent curBlob = currentBlobEvents.ElementAt(i);

                var nearby = tree.Nearest(curBlob.Location, maxRadius, 1);
                if (nearby.Count > 0)
                {
                    var nearestBlob = nearby.First().Node.Value;
                    curBlob.Event = BlobEventType.Move;
                    curBlob.Id = nearestBlob.Id;
                    curBlob.Origin = nearestBlob.Origin == null ? nearestBlob.Location : nearestBlob.Origin;
                    nearestBlob.Tracked = true;
                }
            }
            
            // UP EVENTS
            // All previous blobs which are NOT tracked, have disappeared.
            foreach (BlobEvent evt in lastBlobEvents)
            {
                if (!evt.Tracked)
                {
                    var newEvt = new BlobEvent() { Location = evt.Location, Id = evt.Id, Origin = evt.Origin, Event = BlobEventType.Up };
                    currentBlobEvents.Add(newEvt);
                }
            }

            // Give each DOWN event a proper, new ID.
            foreach (BlobEvent evt in currentBlobEvents)
            {
                evt.Id = generateId();
            }
            return currentBlobEvents.ToArray();
        }
    }
}

// Deprecated, pre-KD-Tree code below. Kept in case KD trees dont turn out to work well.
/*
// Keep track of closest blob in LastBlobs.
var closestEvent = new Tuple<double, BlobEvent, int>(double.MaxValue, null, -1);

for(int j = 0; j < LastBlobs.Count; j++) {
    BlobEvent lastBlob = LastBlobs.ElementAt(j);

    // Distance between current blob, and last blob.
    double dist = Math.Sqrt(Math.Pow(curBlob.Blob.Centroid.X - lastBlob.Blob.Centroid.X, 2.0f) +
                 Math.Pow(curBlob.Blob.Centroid.Y - lastBlob.Blob.Centroid.Y, 2.0f));

    if (dist < maxRadius && (closestEvent.Item2 == null || dist <= closestEvent.Item1))
    {
        // The amount moved is so little, do not bother to register it.
        if(dist < minRadius) {
            curBlob.Event = BlobEventType.FilterOut;
        } else {
            closestEvent = new Tuple<double, BlobEvent, int>(dist, lastBlob, j);
            curBlob.Event = BlobEventType.Move;
            curBlob.Id = lastBlob.Id;
            curBlob.Origin = lastBlob.Origin == null ? lastBlob.Blob.Centroid : lastBlob.Origin;
        }
    }
}
                
// Previous blob is now tracked.
if (closestEvent.Item2 != null) lastBlobsTracked[closestEvent.Item3] = true;
*/
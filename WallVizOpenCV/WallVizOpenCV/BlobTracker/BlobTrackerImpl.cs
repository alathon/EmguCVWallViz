﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Cvb;
using System.Drawing;
using Accord.MachineLearning;
using Accord.MachineLearning.Structures;

namespace WallVizOpenCV.BlobTracker
{
    public class BlobTrackerImpl
    {
        public List<BlobEvent> lastBlobEvents { get; private set; }
        public List<BlobEvent> currentBlobEvents { get; private set; }

        public BlobTrackerImpl() {
            this.lastBlobEvents = new List<BlobEvent>();
            this.currentBlobEvents = new List<BlobEvent>();
        }

        public BlobEvent[] NewFrame(double[][] frame)
        {
            lastBlobEvents = new List<BlobEvent>(currentBlobEvents);
            currentBlobEvents = new List<BlobEvent>();
            return track(frame);
        }

        private KDTree<BlobEvent> makeKDTree(List<BlobEvent> events)
        {
            KDTree<BlobEvent> tree = new KDTree<BlobEvent>(2);
            foreach (BlobEvent evt in events)
            {
                if (evt.Event == BlobEventType.Up) continue;
                tree.Add(evt.Location, evt);
            }
            return tree;
        }

        private string generateId()
        {
            return System.Guid.NewGuid().ToString();
        }

        /** Frame is an array of X/Y coordinate locations, where each location is represented
         * as a double[2].
         */
        private BlobEvent[] track(double[][] frame)
        {      
            double maxRadius = 200f; // max distance between two blobs for them to be considered the same.
            
            // DOWN EVENTS
            // For each location in frame, add a new DOWN BlobEvent.
            for(int i = 0; i < frame.GetLength(0); i++) {
                var blob = new BlobEvent() { Location = frame[i], Event = BlobEventType.Down, Origin = frame[i] };
                currentBlobEvents.Add(blob);
            }

            // If this is not the first frame...
            if (lastBlobEvents.Count > 0)
            {
                /* Debugging ;)
                Console.WriteLine("Last blobs: ");
                for (int i = 0; i < lastBlobEvents.Count; i++)
                {
                    Console.WriteLine("\t{0} @ {1} ({2})", lastBlobEvents.ElementAt(i).Id, 
                        BlobTrackerSampleData.printLoc(lastBlobEvents.ElementAt(i).Location),
                        lastBlobEvents.ElementAt(i).Event);
                }

                Console.WriteLine("Current blobs: ");
                for (int i = 0; i < currentBlobEvents.Count; i++)
                {
                    Console.WriteLine("\t{1}", currentBlobEvents.ElementAt(i).Id,
                        BlobTrackerSampleData.printLoc(currentBlobEvents.ElementAt(i).Location));
                }
                 */
                // MOVE EVENTS
                // Find blobs within a distance of prior blobs, and consider those to be MOVE instead of DOWN.
                // Construct a 2-dimensional KD-Tree of the blob events from last frame.
                KDTree<BlobEvent> tree = makeKDTree(lastBlobEvents);

                // We keep track of blobs from the last frame we've already 'used' for a MOVE.
                var trackedBlobs = new List<BlobEvent>();

                // For each current blob, find the closest unused blob in the last blob events, if any.
                for (int i = 0; i < currentBlobEvents.Count; i++)
                {
                    BlobEvent curBlob = currentBlobEvents.ElementAt(i);
                    var nearby = tree.Nearest(curBlob.Location, maxRadius);

                    if (nearby.Count > 0)
                    {
                        foreach (var nodeDist in nearby)
                        {
                            BlobEvent nodeBlob = nodeDist.Node.Value;
                            if (trackedBlobs.Contains(nodeBlob))
                            {
                                continue;
                            }
                            curBlob.Event = BlobEventType.Move;
                            curBlob.Id = nodeBlob.Id;
                            curBlob.Origin = nodeBlob.Origin == null ? nodeBlob.Location : nodeBlob.Origin;
                            trackedBlobs.Add(nodeBlob);
                            break;
                        }
                    }
                }

                // UP EVENTS
                // All previous blobs which are NOT tracked, have disappeared.
                // TODO: Allow blobs to go untracked for X frames, in case we drop some. Otherwise, touch is too sensitive
                // to momentarily not detecting a finger.
                foreach (BlobEvent evt in lastBlobEvents)
                {
                    if (!trackedBlobs.Contains(evt) && evt.Event != BlobEventType.Up)
                    {
                        var newEvt = new BlobEvent() { Location = evt.Location, Id = evt.Id, Origin = evt.Origin, Event = BlobEventType.Up };
                        currentBlobEvents.Add(newEvt);
                    }
                }
            }

            // Give each DOWN event an ID.
            foreach (BlobEvent evt in currentBlobEvents.FindAll((b) => b.Event == BlobEventType.Down))
            {
                evt.Id = generateId();
            }
            return currentBlobEvents.ToArray();
        }
    }
}
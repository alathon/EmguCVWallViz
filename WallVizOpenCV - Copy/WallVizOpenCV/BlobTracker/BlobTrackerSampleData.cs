using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WallVizOpenCV.BlobTracker
{
    public class BlobTrackerSampleData
    {
        public static string printLoc(double[] loc)
        {
            return "(" + loc[0] + " " + loc[1] + ")";
        }

        public static void Simulate()
        {
            var tracker = new BlobTrackerImpl();
            for (int i = 0; i < frames.GetLength(0); i++)
            {
                Console.WriteLine("----------- FRAME " + i + "-----------");
                double[][] frame = frames[i];
                BlobEvent[] events = tracker.NewFrame(frame);
                foreach (var evt in events)
                {
                    var loc = "(" + evt.Location[0] + " " + evt.Location[1] + ")";
                    var origin = "(" + evt.Origin[0] + " " + evt.Origin[1] + ")";
                    Console.WriteLine("ID: {0} Location: {1} Origin: {2} Type: {3}", evt.Id, loc, origin, evt.Event);
                }
            }
        }

        public static double[][][] frames = new double[][][] {
            new double[][] {
                new double[2] { 1.0f, 1.0f },
            },

            new double[][] {
                new double[2] { 1.5f, 1.5f },
            },

            new double[][] {
                new double[2] { 2.5f, 3f },
            },

            new double[][] {
                new double[2] { 2.5f, 3f },
                new double[2] { 1f, 1f },
            },

            new double[][] {
                new double[2] { 2.5f, 3f },
                new double[2] { 1f, 1f },
            },

            new double[][] {
                new double[2] { 2.5f, 3f },
                new double[2] { 1f, 1f },
            },

            new double[][] {
                new double[2] { 3.5f, 4f },
                new double[2] { 2f, 2f },
            },

            new double[][] {
                new double[2] { 2f, 2f },
            },

            new double[][] {
            },
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Connexion;

namespace WallVizOpenCV.BlobTracker
{
    public enum BlobEventType { Down, Up, Move, FilterOut }

    /*
    This class should add and set the exact same fields as
    BlobEventObject does on the Java side of things.
    */
    public class BlobEvent : Connexion.ITransferable
    {
        public double[] Location;
        public double[] Origin;
        public string Id = "Unknown ID";
        public BlobEventType Event;
        public bool Tracked = false;

        public void GetStreamData(NetworkStreamInfo info)
        {
            info.AddValue("EventName", Event.ToString());
            info.AddValue("Id", Id);
            info.AddValue("LocationX", Location[0]);
            info.AddValue("LocationY", Location[1]);
            info.AddValue("OriginX", Origin[0]);
            info.AddValue("OriginY", Origin[1]);
        }

        public BlobEvent()
        {

        }
        public BlobEvent(NetworkStreamInfo info)
        {
            try
            {
                this.Id = info.GetString("Id");
                this.Event = (BlobEventType) Enum.Parse(typeof(BlobEventType), info.GetString("EventName"));
                this.Location = new double[2] { info.GetDouble("LocationX"), info.GetDouble("LocationY") };
                this.Origin = new double[2] { info.GetDouble("OriginX"), info.GetDouble("OriginY") };
            } catch (DecodingException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

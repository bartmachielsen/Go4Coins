using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPEindopdracht.DataConnections;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht.Assignments
{
    class PhotoAssignment : Assignment
    {
        private GoogleStreetviewConnector _connector;
        public PhotoAssignment()
        {
            _connector = new GoogleStreetviewConnector();
        }

        public override void FillDescription()
        {
            throw new NotImplementedException();
        }

        public override async Task FillTarget(List<Place> places, GCoordinate currentPosition)
        {
            List<string> urls = new List<string>();
            while (Target == null && urls.Count == 0)
            {
                await base.FillTarget(places, currentPosition);
                if(Target != null && Target.Length > 0)
                    urls = await _connector.GetURLToSavePicture(Target[0]);
            }
        }
    }
}

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

        public override string GetDescription()
        {
            throw new NotImplementedException();
        }

        public override async Task FillTarget(List<Place> places, GCoordinate currentPosition)
        {
            string urls = null;
            while (Targets == null && urls == null)
            {
                await base.FillTarget(places, currentPosition);
                if(Targets != null && Targets.Length > 0)
                    urls = await _connector.GetURLToSavePicture(Targets[0]);
            }
        }
    }
}

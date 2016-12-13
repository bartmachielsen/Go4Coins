using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht.DataConnections
{
    class WikipediaConnector : HttpConnector
    {
        public WikipediaConnector() : base("http://nl.wikipedia.com/")
        {
            SourcePriority = Priority.Normal;
        }

        public async Task<List<Place>> GetPlaces(int diameter, GCoordinate coordinate)
        {
           throw new NotImplementedException();
        }
    }
}

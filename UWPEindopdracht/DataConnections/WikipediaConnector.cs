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
        public WikipediaConnector() : base("https://nl.wikipedia.org/w/api.php?")
        {
            SourcePriority = Priority.Normal;
        }

        public async Task<List<Place>> GetPlaces(int diameter, GCoordinate coordinate)
        {
           Uri uri = new Uri($"{host}action=query&list=geosearch&gscoord={coordinate.longi}%7C{coordinate.lati}gsradius={diameter}&format=json");
            var response = await get(uri);
            System.Diagnostics.Debug.WriteLine(response);
            throw new NotImplementedException();
        }
    }
}

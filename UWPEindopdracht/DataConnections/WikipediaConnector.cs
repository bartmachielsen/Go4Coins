using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.JSON;
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
            var uri = new Uri($"{Host}action=query&list=geosearch&gscoord={coordinate.lati}%7C{coordinate.longi}&gsradius={diameter}&format=json");
            var response = await Get(uri);
            return response == null ? new List<Place>() : WikipediaParser.GetPlaces(response);
        }
    }
}

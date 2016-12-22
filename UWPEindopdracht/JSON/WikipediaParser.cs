using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht.JSON
{
    static class WikipediaParser
    {
        public static List<Place> GetPlaces(string response)
        {
            List<Place> places = new List<Place>();

            dynamic json = JsonConvert.DeserializeObject(response);

            foreach (dynamic jsonplace in json.query.geosearch)
            {
                places.Add(new Place(HttpConnector.Priority.Normal, false, (double) jsonplace.dist,
                    new GCoordinate((double) jsonplace.lat, (double) jsonplace.lon), (string) jsonplace.title, null,
                    null, null, null, null, null));
               
            }
            return places;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht.DataConnections
{
    /// <summary>
    /// Class for connecting to the web api from google places
    /// </summary>
    class GooglePlacesConnector : HttpConnector, ApiKeyConnector
    {
        /// <summary>
        /// <see cref="ApiKeyConnector.apiKey"/>
        /// </summary>
        public string apiKey { get; set; } = "AIzaSyALNAeJEW5aA8D8AdXE4CDDXX4IYh5o1Ns";

        public async Task<Place[]> GetPlaces(int diameter, GCoordinate coordinate)
        {
            Uri uriPlaces = new Uri($"http://maps.googleapis.com/maps/api/place/nearbysearch/json?location={coordinate}&radius={diameter}&key={apiKey}");
            return JsonConverter(await get(uriPlaces));
        }

        public Place[] GetPlaces(string City)
        {
            throw new NotImplementedException();
        }
    }
}
    
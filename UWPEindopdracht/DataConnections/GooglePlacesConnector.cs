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
            return GooglePlacesParser.GetPlaces(await get(uriPlaces));
        }

        public async Task<Place[]> GetPlaces(string city)
        {
            Uri uriPlaces = new Uri($"https://maps.googleapis.com/maps/api/place/textsearch/json?query={city}&key={apiKey}");
            return GooglePlacesParser.GetPlaces(await get(uriPlaces));
        }
    }
}
    
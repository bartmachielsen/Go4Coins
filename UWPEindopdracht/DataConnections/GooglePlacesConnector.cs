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
        public static int waitingTime = 4000;
        /// <summary>
        /// <see cref="ApiKeyConnector.apiKey"/>
        /// </summary>
        public string apiKey { get; set; } = "AIzaSyALNAeJEW5aA8D8AdXE4CDDXX4IYh5o1Ns";

        public GooglePlacesConnector() : base("https://maps.googleapis.com")
        {
        }

        public async Task<List<Place>> GetPlaces(int diameter, GCoordinate coordinate, string pagetoken = null, string firstToken = null)
        {
            Uri uriPlaces = new Uri($"{host}/maps/api/place/nearbysearch/json?location={coordinate.lati},{coordinate.longi}&radius={diameter}&key={apiKey}");
            if (pagetoken != null)
                uriPlaces = new Uri($"{host}/maps/api/place/nearbysearch/json?pagetoken={pagetoken}&key={apiKey}");
            string response = await get(uriPlaces);
            if (GooglePlacesParser.GetStatus(response) == "OVER_QUERY_LIMIT")
            {
                throw new ApiLimitReached();
            }else if (GooglePlacesParser.GetStatus(response) == "INVALID_REQUEST")
            {
                await Task.Delay(waitingTime);
                return await GetPlaces(diameter, coordinate, pagetoken, firstToken);
            }
            string nextPage = GooglePlacesParser.NextPage(response);
            var list = GooglePlacesParser.GetPlaces(response);
            if (nextPage != null && nextPage != firstToken)
            {
                if (firstToken == null)
                    firstToken = nextPage;
                list.AddRange(await GetPlaces(diameter, coordinate, nextPage, firstToken));
            }

            return list;
        }

       
        public async Task<List<Place>> GetPlaces(string city)
        {
            Uri uriPlaces = new Uri($"{host}/maps/api/place/textsearch/json?query={city}&key={apiKey}");
            return GooglePlacesParser.GetPlaces(await get(uriPlaces));
        }
    }

    
}
    
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.JSON;
using UWPEindopdracht.Places;

namespace UWPEindopdracht.DataConnections
{
    /// <summary>
    ///     Class for connecting to the web api from google places
    /// </summary>
    internal class GooglePlacesConnector : HttpConnector, ApiKeyConnector
    {
        private static int waitingTime = 4000;

        public GooglePlacesConnector() : base("https://maps.googleapis.com/maps/api/place")
        {
            SourcePriority = Priority.High;
        }

        /// <summary>
        ///     <see cref="ApiKeyConnector.apiKey" />
        /// </summary>
        public string apiKey { get; set; } = "AIzaSyALNAeJEW5aA8D8AdXE4CDDXX4IYh5o1Ns";

        public async Task<List<Place>> GetPlaces(int diameter, GCoordinate coordinate, string pagetoken = null,
            string firstToken = null)
        {
            var uriPlaces =
                new Uri(
                    $"{host}/nearbysearch/json?location={coordinate.lati},{coordinate.longi}&radius={diameter}&key={apiKey}");
            if (pagetoken != null)
                uriPlaces = new Uri($"{host}/nearbysearch/json?pagetoken={pagetoken}&key={apiKey}");
            var response = await get(uriPlaces);
            switch (GooglePlacesParser.GetStatus(response))
            {
                case "OVER_QUERY_LIMIT":
                    throw new ApiLimitReached();
                case "INVALID_REQUEST":
                    await Task.Delay(waitingTime);
                    return await GetPlaces(diameter, coordinate, pagetoken, firstToken);
                case "INVALID":
                    return new List<Place>();
              
                    
            }
            var nextPage = GooglePlacesParser.NextPage(response);
            var list = GooglePlacesParser.GetPlaces(response);
            if ((nextPage == null) || (nextPage == firstToken)) return list;
            if (firstToken == null)
                firstToken = nextPage;
            list.AddRange(await GetPlaces(diameter, coordinate, nextPage, firstToken));
            return list;
        }


        public async Task<string> GetImageURL(Place place, int maxwidth = 360, int maxheight = 0)
        {
            string width = "";
            if (maxwidth != 0)
                width = $"maxwidth={maxwidth}&";
            else if(maxheight != 0)
                width = $"maxheight={maxheight}&";
            var link = $"{host}/photo?{width}photoreference={place.ImageLocation}&key={apiKey}";
            var header = await getHeaders(new Uri(link));
            if (header.IsSuccessStatusCode)
            {
                return link;
            }
            return null;
        }
    }
}
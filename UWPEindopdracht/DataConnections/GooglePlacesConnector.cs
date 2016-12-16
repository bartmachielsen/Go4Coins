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
    internal class GooglePlacesConnector : HttpConnector, IApiKeyConnector
    {
        private static int waitingTime = 4000;

        public GooglePlacesConnector() : base("https://maps.googleapis.com/maps/api/place")
        {
            SourcePriority = Priority.High;
        }

        /// <summary>
        ///     <see cref="IIApiKeyConnector.ApiKey />
        /// </summary>
        public string ApiKey { get; set; } = "AIzaSyALNAeJEW5aA8D8AdXE4CDDXX4IYh5o1Ns";

        public async Task<List<Place>> GetPlaces(int diameter, GCoordinate coordinate, string pagetoken = null,
            string firstToken = null)
        {
            var uriPlaces =
                new Uri(
                    $"{Host}/nearbysearch/json?location={coordinate.lati},{coordinate.longi}&radius={diameter}&key={ApiKey}");
            if (pagetoken != null)
                uriPlaces = new Uri($"{Host}/nearbysearch/json?pagetoken={pagetoken}&key={ApiKey}");
            var header = await Get(uriPlaces);
            if(!header.IsSuccessStatusCode)
                return new List<Place>();
            var response = await ConvertResponseMessageToContent(header);
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
            var link = $"{Host}/photo?{width}photoreference={place.ImageLocation}&key={ApiKey}";
            var header = await Get(new Uri(link));
            if (header.IsSuccessStatusCode)
            {
                return link;
            }
            return null;
        }
    }
}
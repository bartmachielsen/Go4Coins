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
    class GoogleStreetviewConnector : HttpConnector, IApiKeyConnector
    {
        public string ApiKey { get; set; } = "AIzaSyALNAeJEW5aA8D8AdXE4CDDXX4IYh5o1Ns";
        public int[] size = new[] {600, 300};
        public GoogleStreetviewConnector() : base("https://maps.googleapis.com/maps/api/streetview")
        {
        }

        public async Task<string> GetURLToSavePicture(Place place)
        {
            List<string> workingPictures = new List<string>();
            List<GCoordinate> coordinates = new List<GCoordinate> () { place.Location};
            if(place.Viewports != null)
                coordinates.AddRange(place.Viewports);
            foreach (var viewport in coordinates)
            {
                string url =
                    $"{Host}/metadata?size={size[0]}x{size[1]}&location={viewport.lati},{viewport.longi}&key={ApiKey}";
                var response = await ConvertResponseMessageToContent(await Get(new Uri(url)));
                if(GooglePlacesParser.GetStatus(response) == "OK")
                    return $"{Host}?size={size[0]}x{size[1]}&location={viewport.lati},{viewport.longi}&key={ApiKey}";
            }
            return null;
        }
        
    }
}

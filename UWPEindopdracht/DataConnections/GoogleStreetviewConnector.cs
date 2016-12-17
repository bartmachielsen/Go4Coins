using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPEindopdracht.GPSConnections;
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

        public async Task<List<string>> GetURLToSavePicture(Place place)
        {
            List<string> workingPictures = new List<string>();
            List<GCoordinate> coordinates = new List<GCoordinate>();
            if (place.Viewports != null)
                coordinates.AddRange(place.Viewports);
            coordinates.Add(place.Location);
            foreach (var viewport in coordinates)
            {
                string url =
                    $"{Host}?size={size[0]}x{size[1]}&location={viewport.lati},{viewport.longi}&key={ApiKey}";
                if ((await Get(new Uri(url))).IsSuccessStatusCode)
                    workingPictures.Add(url);
            }

            return workingPictures;
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using UWPEindopdracht.Places;

namespace UWPEindopdracht.DataConnections
{
    class ImageLoader
    {
        public static int MaxHeight = 100;
        public static async Task<string> GetBestUrlFromPlace(Assignment assignment)
        {
            if (!assignment.ShowPicture || assignment.Targets == null) return null;
            return await GetBestUrlFromPlace(assignment.Targets[0]);
        }

        public static async Task<string> GetBestUrlFromPlace(Place place)
        {
            if (place.ImageLocation != null)
            {
                string url = await new GooglePlacesConnector().GetImageURL(place, maxheight: MaxHeight);
                if (url != null)
                    return url;
            }
            return await new GoogleStreetviewConnector().GetURLToSavePicture(place);
        }
    }
}

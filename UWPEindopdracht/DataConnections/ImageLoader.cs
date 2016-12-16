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
            if (!assignment.ShowPicture) return null;
            if (assignment.Target[0].ImageLocation != null)
            {
                string url = await new GooglePlacesConnector().GetImageURL(assignment.Target[0], maxheight:MaxHeight);
                if (url != null)
                    return url;
                
            }
            var urls = await new GoogleStreetviewConnector().GetURLToSavePicture(assignment.Target[0]);
            if (urls.Count > 0)
                return urls.ElementAt(0);
            return null;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht.JSON
{
    class GooglePlacesParser
    {
        public static Place[] GetPlaces(string response)
        {
            dynamic json = JsonConvert.DeserializeObject(response);
            System.Diagnostics.Debug.WriteLine(response);
            // where is this one created ?
            Place[] placesToSend = null;

            /// why these variables that are used in a lower piece ?
            var name = "";
            var lati = 0.0;
            var longi = 0.0;
            var reference = "";
            var icon = "";
            string[] types = null;
            var count = 0;
            
            if (!(json is JObject))
            {
                System.Diagnostics.Debug.WriteLine(response);
                return null;
            }
            // is not possible to parse because it is not a JArray!
            JArray jsonval = JArray.Parse(json);
            // why this dynamic and not a forloop with the created jarray?
            dynamic jsonplaces = jsonval;
            foreach (dynamic jsonplace in jsonplaces)
            {
                // why save temp and not immediately in constructor of place? (PLEASE NO EXTRA SAVE)
                name = jsonplace.name;
                reference = jsonplace.photos.photo_reference;
                lati = jsonplace.geometry.location.lat;
                longi = jsonplace.geometry.location.lng;
                icon = jsonplace.icon;
                // jsonplace.type.Count is not possible! replace with a forloop! 
                for (int i = 0; i < jsonplace.type.Count; i++)
                {
                    // types is null because never initialized
                    // jsonplace.type is not possible because it is a whole array that is saved everytime
                    types[i] = jsonplace.type;
                }
                if (types != null)
                {
                    // nullpointer because placestosend is null
                    placesToSend[count] = new Place(new GCoordinate(lati, longi), name, 0, types, $"{lati}, {longi}", icon, reference);
                    count++;
                }
                // why kill the whole loop if one item does not containt types ?
                else return null;
            }
            // why first check if object is null and if null return null ? bullshit extra code
            if (placesToSend != null)
            {
                return placesToSend;
            }
            else return null;
        }
    }
}

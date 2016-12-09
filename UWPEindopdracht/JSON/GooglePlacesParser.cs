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
            Place[] placesToSend = null;

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
   
            JArray jsonval = JArray.Parse(json);
            dynamic jsonplaces = jsonval;
            foreach (dynamic jsonplace in jsonplaces)
            {
                name = jsonplace.name;
                reference = jsonplace.photos.photo_reference;
                lati = jsonplace.geometry.location.lat;
                longi = jsonplace.geometry.location.lng;
                icon = jsonplace.icon;
                for (int i = 0; i < jsonplace.type.Count; i++)
                {
                    types[i] = jsonplace.type;
                }
                if (types != null)
                {
                    placesToSend[count] = new Place(new GCoordinate(lati, longi), name, 0, types, $"{lati}, {longi}", icon, reference);
                    count++;
                }
                else return null;
            }
            if (placesToSend != null)
            {
                return placesToSend;
            }
            else return null;
        }
    }
}

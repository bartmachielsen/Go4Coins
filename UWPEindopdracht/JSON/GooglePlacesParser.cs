using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            string name;
            double lati;
            double longi;
            string reference;
            string icon;

            if (!(json is JObject))
            {
                System.Diagnostics.Debug.WriteLine(response);
                return null;
            }

            var obj = (JObject)json;
            Place[] places;

            if (json == null)
            {
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
                new Place(new GCoordinate(lati, longi), )
            }
        }
    }
}

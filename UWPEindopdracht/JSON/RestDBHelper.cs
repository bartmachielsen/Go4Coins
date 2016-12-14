using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Multiplayer;

namespace UWPEindopdracht.JSON
{
    internal class RestDBHelper
    {
        public static string ConvertUsername(User user)
        {
            return JsonConvert.SerializeObject(new
            {
              data = JsonConvert.SerializeObject(user)
            });
        }

        public static List<User> getUsernames(string response)
        {
            List<User> users = new List<User>();
            dynamic json = JsonConvert.DeserializeObject(response);
            foreach (var jsonelement in json)
            {
                if (((JToken) jsonelement)["data"] != null)
                {
                    users.Add(new User((string)jsonelement._id, (string)jsonelement.data.Name, new GCoordinate((double)jsonelement.data.location.lati, (double)jsonelement.data.location.longi))
                    {
                        lastSynced = DateTime.Parse((string)jsonelement.data.lastSynced)
                    });
                }
            }
            return users;
        }

        public static string getID(string response)
        {
            dynamic json = JsonConvert.DeserializeObject(response);
            return json._id;
        }
    }
}
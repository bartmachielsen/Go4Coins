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

        public static List<User> getUsernames(string response, List<User> users)
        {
            dynamic json = JsonConvert.DeserializeObject(response);
            if (json == null)
            {
                return users;
            }
            foreach (var jsonelement in json)
            {
                if (((JToken) jsonelement)["data"] != null)
                {
                    var user = new User(
                        (string)jsonelement._id,
                        (string)jsonelement.data.Name,
                        new GCoordinate((double)jsonelement.data.location.lati, 
                                        (double)jsonelement.data.location.longi));
                    DateTime time;
                    if(DateTime.TryParse((string)jsonelement.data.lastSynced, out time))
                        user.lastSynced = time;
                    else
                    {
                        try
                        {
                            user.lastSynced = (DateTime) jsonelement.data.lastSynced;
                        }
                        catch (Exception) { }
                    }
                    System.Diagnostics.Debug.WriteLine(user.lastSynced);
                    bool exists = false;
                    foreach (var existuser in users)
                    {
                        if (existuser.id == user.id)
                        {
                            exists = true;
                            existuser.location = user.location;
                            existuser.Name = user.Name;
                            existuser.lastSynced = user.lastSynced;
                            // TODO add changeble things
                        }
                    }
                    if(!exists)
                        users.Add(user);
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
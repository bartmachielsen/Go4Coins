﻿using System;
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
            foreach (var jsonelement in json)
            {
                try
                {
                    if (((JToken) jsonelement)["data"] != null)
                    {
                        var user = getUsername(JsonConvert.SerializeObject(jsonelement));
                        var exists = false;
                        foreach (var existuser in users)
                        {
                            if (existuser.id == user.id)
                            {
                                exists = true;
                                existuser.LastState = LastState.Updated;
                                existuser.Location = user.Location;
                                existuser.Name = user.Name;
                                existuser.LastSynced = user.LastSynced;
                            }
                        }
                        if (!exists)
                        {
                            user.LastState = LastState.Online;
                            users.Add(user);
                        }
                    }
                }
                catch (Exception e)
                {
                    
                }
            }
            return users;
        }

        public static string getID(string response)
        {
            dynamic json = JsonConvert.DeserializeObject(response);
            return json._id;
        }

        public static bool CheckErrors(string response)
        {
            if (response == null)
                throw new NoResponseException();

            dynamic json = JsonConvert.DeserializeObject(response);

            if (json.GetType() == typeof(JToken))
            {
                if (((JToken) json)["message"] != null)
                {
                    string message = (string) json.message;
                    if (message == "API-key is not valid")
                        throw new InvalidApiKeyException();
                    if(message == "Nothing was updated. Check Query.") 
                        throw new CannotUploadException();
                }
                if (!((JObject) json).HasValues)
                    throw new NoResponseException();
            }
            return true;
        }

        public static string ConvertReward(Reward reward)
        {
            return JsonConvert.SerializeObject(reward);
        }

        public static List<Reward> GetRewards(string response)
        {
            List<Reward> rewards = new List<Reward>();
            dynamic json = JsonConvert.DeserializeObject(response);
            foreach (var jsonelement in json)
            {
                rewards.Add(new Reward(
                    (string)jsonelement._id, 
                    (string)jsonelement.Name, 
                    (string)jsonelement.ImageLocation,
                    (string)jsonelement.Description, 
                    (RewardValue)jsonelement.Value));
            }
            return rewards;
        }

        public static User getUsername(string response)
        {
            dynamic data = JsonConvert.DeserializeObject(response);
            User user = JsonConvert.DeserializeObject<User>(JsonConvert.SerializeObject(data.data));
            user.id = (string) data._id;
            return user;
        }
    }

    class CannotUploadException : Exception
    {
        
    }
}
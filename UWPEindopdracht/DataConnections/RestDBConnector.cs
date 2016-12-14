using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using UWPEindopdracht.JSON;
using UWPEindopdracht.Multiplayer;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

namespace UWPEindopdracht.DataConnections
{
    class RestDBConnector : HttpConnector,ApiKeyConnector
    {
        public string apiKey { get; set; } = "711dc584f7d33bf508b643a165c95bc9a4129";

        public RestDBConnector() : base("https://uwpeindopdracht-429b.restdb.io/rest")
        {
            
        }

        public async Task<List<User>> GetUsers(List<User> current )
        {
            Uri uri = new Uri($"{host}/multiplayer?apikey={apiKey}");
            string response = await get(uri);
            try
            {
                RestDBHelper.CheckErrors(response);
                return RestDBHelper.getUsernames(response, current);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Getting users failed safely!");
            }
            return current;
        }

        public async Task<User> UploadUser(User user)
        {
            user.lastSynced = DateTime.Now;
            Uri uri = new Uri($"{host}/multiplayer?apikey={apiKey}");
            string response = await post(uri, new HttpStringContent(RestDBHelper.ConvertUsername(user), UnicodeEncoding.Utf8, "application/json"));
            RestDBHelper.CheckErrors(response);
            user.id = RestDBHelper.getID(response);
            return user;
        }

        public async Task UpdateUser(User user)
        {
            user.lastSynced = DateTime.Now;
            Uri uri = new Uri($"{host}/multiplayer/{user.id}?apikey={apiKey}");
            string response = await put(uri, new HttpStringContent(RestDBHelper.ConvertUsername(user), UnicodeEncoding.Utf8, "application/json"));
            RestDBHelper.CheckErrors(response);
        }

        public async void UploadReward(Reward reward)
        {
            Uri uri = new Uri($"{host}/rewards?apikey={apiKey}");
            string response =
                await
                    post(uri,
                        new HttpStringContent(RestDBHelper.ConvertReward(reward), UnicodeEncoding.Utf8,
                            "application/json"));
        }

        public async Task<List<Reward>> GetRewards()
        {
            Uri uri = new Uri($"{host}/rewards?apikey={apiKey}");
            string response = await get(uri);
            RestDBHelper.CheckErrors(response);
            return RestDBHelper.GetRewards(response);
        }
    }
}

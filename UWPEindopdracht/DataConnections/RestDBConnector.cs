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
    class RestDBConnector : HttpConnector,IApiKeyConnector
    {
        public string ApiKey { get; set; } = "711dc584f7d33bf508b643a165c95bc9a4129";

        public RestDBConnector() : base("https://uwpeindopdracht-429b.restdb.io/rest")
        {
            
        }

        public async Task<List<User>> GetUsers(List<User> current )
        {
            Uri uri = new Uri($"{Host}/multiplayer?apikey={ApiKey}");
            var header = await Get(uri);
            try
            {
                var response = await ConvertResponseMessageToContent(header);
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
            Uri uri = new Uri($"{Host}/multiplayer?apikey={ApiKey}");
            user.id = null;
            while (user.id == null)
            {
                try
                {
                    var header =
                        await
                            Post(uri,
                                new HttpStringContent(RestDBHelper.ConvertUsername(user), UnicodeEncoding.Utf8,
                                    "application/json"));
                    var response = await ConvertResponseMessageToContent(header);
                    System.Diagnostics.Debug.WriteLine($"UPLOADING USER {user}");
                    if (!header.IsSuccessStatusCode) continue;
                    RestDBHelper.CheckErrors(response);
                    user.id = RestDBHelper.getID(response);
                }
                catch (NoResponseException)
                {

                }
            }
            return user;
        }

        public async Task<User> GetUser(string id)
        {
            
        }
        public async Task UpdateUser(User user)
        {
            user.lastSynced = DateTime.Now;
            Uri uri = new Uri($"{Host}/multiplayer/{user.id}?apikey={ApiKey}");
            while (true)
            {
                var header =
                    await
                        Put(uri,
                            new HttpStringContent(RestDBHelper.ConvertUsername(user), UnicodeEncoding.Utf8,
                                "application/json"));
                System.Diagnostics.Debug.WriteLine($"UPDATING USER {user}");
                if (header.IsSuccessStatusCode)
                    return;
            }
        }

        public async void UploadReward(Reward reward)
        {
            Uri uri = new Uri($"{Host}/rewards?apikey={ApiKey}");
            string response =
                await
                    Post(uri,
                        new HttpStringContent(RestDBHelper.ConvertReward(reward), UnicodeEncoding.Utf8,
                            "application/json"));
        }

        public async Task<List<Reward>> GetRewards()
        {
            Uri uri = new Uri($"{Host}/rewards?apikey={ApiKey}");
            string response = await Get(uri);
            RestDBHelper.CheckErrors(response);
            return RestDBHelper.GetRewards(response);
        }
    }
}

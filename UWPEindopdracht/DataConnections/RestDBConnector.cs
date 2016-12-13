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

        public async Task<List<User>> GetUsers()
        {
            Uri uri = new Uri($"{host}/multiplayer?apikey={apiKey}");
            string response = await get(uri);
            return RestDBHelper.getUsernames(response);
        }

        public async Task<User> UploadUser(User user)
        {
            user.lastSynced = DateTime.Now;
            Uri uri = new Uri($"{host}/multiplayer?apikey={apiKey}");
            string response = await post(uri, new HttpStringContent(RestDBHelper.ConvertUsername(user), UnicodeEncoding.Utf8, "application/json"));
            user.id = RestDBHelper.getID(response);
            return user;
        }

        public async void UpdateUser(User user)
        {
            user.lastSynced = DateTime.Now;
            Uri uri = new Uri($"{host}/multiplayer/{user.id}?apikey={apiKey}");
            string response = await put(uri, new HttpStringContent(RestDBHelper.ConvertUsername(user), UnicodeEncoding.Utf8, "application/json"));
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace UWPEindopdracht
{
    /// <summary>
    /// class that connects to a http rest api
    /// </summary>
    class HttpConnector
    {
        /// <summary>
        /// Httpclient class for putting the requests to the server
        /// </summary>
        protected HttpClient client;

        /// <summary>
        /// the host address of the target server
        /// default: localhost
        /// </summary>
        private string host { get; set; }
        /// <summary>
        /// the host port of the target server
        /// default: 80
        /// </summary>
        private int port { get; set; }

        /// <summary>
        /// Default constructor for creating a HttpConnector class
        /// </summary>
        /// <param name="host">target host <seealso cref="host"/></param>
        /// <param name="port">target port of the server<seealso cref="port"/></param>
        public HttpConnector(string host = "localhost", int port = 80)
        {
            this.host = host;
            this.port = port;
        }

        /// <summary>
        /// Method for putting data in the target server, PUT request in which content is uploaded
        /// </summary>
        /// <param name="link">The target uri to the server which contains the hostaddress and port and targetdirectories</param>
        /// <param name="content"> the content that will been uploaded to the specified address</param>
        /// <returns>returns response string from the targeted server</returns>
        protected async Task<string> put(Uri link, IHttpContent content)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            try
            {
                var response = await client.PutAsync(link, content);
                if (response == null || !response.IsSuccessStatusCode)
                    return null;
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                return null;
            }
        }
        /// <summary>
        /// Method for getting data from the target server, GET request in which content is been downloaded
        /// </summary>
        /// <param name="link">The target uri to the server which contains the hostaddress and port and targetdirectories</param>
        /// <returns>returns response string from the targeted server</returns>
        protected async Task<string> get(Uri link)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            try
            {
                var response = await client.GetAsync(link);
                if (response == null || !response.IsSuccessStatusCode)
                    return string.Empty;

                string cont = await response.Content.ReadAsStringAsync();
                return cont;
            }
            catch (Exception e)
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// Method for posting data in the target server, POST request in which content is submitted
        /// </summary>
        /// <param name="link">The target uri to the server which contains the hostaddress and port and targetdirectories</param>
        /// <param name="content"> the content that will been uploaded to the specified address</param>
        /// <returns>returns response string from the targeted server</returns>
        protected async Task<string> post(Uri link, IHttpContent content)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            try
            {
                var response = await client.PostAsync(link, content);
                if (response == null || !response.IsSuccessStatusCode)
                    return null;
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    
    /// <summary>
    /// Class for connecting to the web api from google places
    /// </summary>
    class GooglePlacesConnector : HttpConnector,ApiKeyConnector
    {

        /// <summary>
        /// <see cref="ApiKeyConnector.apiKey"/>
        /// </summary>
        public string apiKey { get; set; }



    }


    public interface ApiKeyConnector
    {
        /// <summary>
        /// api key for authorization to the web api
        /// <exception cref="InvalidApiKeyException"> When api key is null or not correct to the required api key</exception>
        /// </summary>
        string apiKey { get; set; }
    }
    /// <summary>
    /// Exception class for exceptions when api key is invalid
    /// </summary>
    class InvalidApiKeyException : Exception
    {

    }
}

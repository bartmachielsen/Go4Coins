using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace UWPEindopdracht
{
    /// <summary>
    /// class that connects to a http rest api
    /// </summary>
    public class HttpConnector
    {
        /// <summary>
        /// Httpclient class for putting the requests to the server
        /// </summary>
        private HttpClient _client;


        public enum Priority
        {
            High, Normal, Low, Outdated
        }

        protected Priority SourcePriority;
        /// <summary>
        /// the host address of the target server
        /// default: localhost
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// the host port of the target server
        /// default: 80
        /// </summary>
        private int _port;

        /// <summary>
        /// Default constructor for creating a HttpConnector class
        /// </summary>
        /// <param name="host">target host <seealso cref="Host"/></param>
        /// <param name="port">target port of the server<seealso cref="_port"/></param>
        public HttpConnector(string host = "localhost", int port = 80, Priority SourcePriority = Priority.Normal)
        {
            this.Host = host;
            this._port = port;
            _client = new HttpClient();
            this.SourcePriority = SourcePriority;
        }

        /// <summary>
        /// Method for putting data in the target server, PUT request in which content is uploaded
        /// </summary>
        /// <param name="link">The target uri to the server which contains the hostaddress and port and targetdirectories</param>
        /// <param name="content"> the content that will been uploaded to the specified address</param>
        /// <returns>returns response string from the targeted server</returns>
        protected async Task<HttpResponseMessage> Put(Uri link, IHttpContent content)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            try
            {
                return await _client.PutAsync(link, content);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                return null;
            }
        }

        protected static async Task<string> ConvertResponseMessageToContent(HttpResponseMessage message)
        {
            return await message.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// Method for getting data from the target server, GET request in which content is been downloaded
        /// </summary>
        /// <param name="link">The target uri to the server which contains the hostaddress and port and targetdirectories</param>
        /// <returns>returns response string from the targeted server</returns>
        protected async Task<HttpResponseMessage> Get(Uri link)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            try
            {
                return await _client.GetAsync(link);
            }
            catch (COMException)
            {
                throw new NoInternetException();
            }
        }
       
        /// <summary>
        /// Method for posting data in the target server, POST request in which content is submitted
        /// </summary>
        /// <param name="link">The target uri to the server which contains the hostaddress and port and targetdirectories</param>
        /// <param name="content"> the content that will been uploaded to the specified address</param>
        /// <returns>returns response string from the targeted server</returns>
        protected async Task<HttpResponseMessage> Post(Uri link, IHttpContent content)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            try
            {
                return await _client.PostAsync(link, content);
            }
            catch (COMException)
            {
                throw new NoInternetException();
            }
        }
    }

    public interface IApiKeyConnector
    {
        /// <summary>
        /// api key for authorization to the web api
        /// <exception cref="InvalidApiKeyException"> When api key is null or not correct to the required api key</exception>
        /// </summary>
        string ApiKey { get; set; }
    }
    /// <summary>
    /// Exception class for exceptions when api key is invalid
    /// </summary>
    class InvalidApiKeyException : Exception
    {

    }
    class ApiLimitReached : Exception
    {

    }

    class NoResponseException : Exception
    {
        
    }

    class NoInternetException : Exception
    {
        
    }
}

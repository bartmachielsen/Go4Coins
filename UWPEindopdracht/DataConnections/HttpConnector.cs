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
        private HttpClient client;


        public enum Priority
        {
            High, Normal, Low, Outdated
        }

        public Priority SourcePriority;
        /// <summary>
        /// the host address of the target server
        /// default: localhost
        /// </summary>
        public string host { get; set; }
        /// <summary>
        /// the host port of the target server
        /// default: 80
        /// </summary>
        public int port { get; set; }

        /// <summary>
        /// Default constructor for creating a HttpConnector class
        /// </summary>
        /// <param name="host">target host <seealso cref="host"/></param>
        /// <param name="port">target port of the server<seealso cref="port"/></param>
        public HttpConnector(string host = "localhost", int port = 80, Priority SourcePriority = Priority.Normal)
        {
            this.host = host;
            this.port = port;
            client = new HttpClient();
            this.SourcePriority = SourcePriority;
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
            return await (await getHeaders(link)).Content.ReadAsStringAsync();
        }
        /// <summary>
        /// Method for getting header of the response from the server
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> getHeaders(Uri link)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            try
            {
                var response = await client.GetAsync(link);
                return response;
            }
            catch (COMException e)
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
            catch (COMException e)
            {
                throw new NoInternetException();
            }
        }
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

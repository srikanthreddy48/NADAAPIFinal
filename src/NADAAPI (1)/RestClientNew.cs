using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NADAAPI
{
    public class RestClientNew
    {

        public RestClientNew(string endpoint, HttpVerb method, string postData)
        {
            this.EndPoint = endpoint;
            this.Method = method;
            this.ContentType = "application/json";
            this.PostData = postData;
            this.Headers = new Dictionary<string, string>();
        }

        public RestClientNew(
            string endpoint,
            HttpVerb method,
            string postData,
            Dictionary<string, string> headerDictionary)
        {
            this.EndPoint = endpoint;
            this.Method = method;
            this.ContentType = "application/json";
            this.PostData = postData;
            this.Headers = headerDictionary;
        }

        public string ContentType { get; set; }

        public string EndPoint { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public HttpVerb Method { get; set; }

        public string PostData { get; set; }

        public async Task<string> MakeRequest()
        {
            return await this.MakeRequest(string.Empty);
        }

        public async Task<string> MakeRequest(string parameters)
        {
            string url = this.EndPoint + parameters;
            if (!string.IsNullOrEmpty(this.PostData) && this.Method == HttpVerb.POST)
            {
                using (HttpClient client = new HttpClient())
                {
                    foreach (var header in this.Headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                    HttpContent cnt = new StringContent(this.PostData, Encoding.UTF8, "application/json");

                    var result = await client.PostAsync(url, cnt);

                    using (HttpContent content = result.Content)
                    {
                        return await content.ReadAsStringAsync();
                    }
                    
                }
            }

            else
            {
                

                using (HttpClient client = new HttpClient())
                {
                    foreach (var header in this.Headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }

                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            if (response.StatusCode.ToString() == "Unauthorized")
                            {
                                return "(401)";
                            }
                            var message = string.Format("Request failed. Received HTTP {0}", response.StatusCode);
                            return message;
                        }

                        using (HttpContent content = response.Content)
                        {
                            return await content.ReadAsStringAsync();
                        }
                    }
                }
            }
        }
    }
}

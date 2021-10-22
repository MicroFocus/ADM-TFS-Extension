using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK
{
    using C = Constants;
    public class RestClient : IClient
    {
        private const string SET_COOKIE = "Set-Cookie";
        private const string XSRF_TOKEN = "XSRF-TOKEN";

        protected readonly Uri _serverUrl; // Example : http://myd-vm21045.swinfra.net:8080/qcbin
        protected IDictionary<string, string> _cookies = new Dictionary<string, string>();
        private readonly Uri _restPrefix;
        private readonly Uri _webuiPrefix;
        private readonly string _clientType;
        private readonly Credentials _credentials;
        private readonly string _xsrfTokenValue;
        private readonly ILogger _logger;
        private string _rawCookies => GetCookiesAsString();

        static RestClient()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public RestClient(string serverUrl, string domain, string project, string clientType, Credentials credentials, ILogger logger)
        {
            _serverUrl = new Uri(serverUrl);
            _clientType = clientType;
            _credentials = credentials;
            _restPrefix = new Uri(_serverUrl.AppendSuffix($"rest/domains/{domain}/projects/{project}"));
            _webuiPrefix = new Uri(_serverUrl.AppendSuffix($"webui/alm/{domain}/{project}"));
            _logger = logger;

            _xsrfTokenValue = Guid.NewGuid().ToString();
            _cookies.Add(XSRF_TOKEN, _xsrfTokenValue);
        }

        public Uri ServerUrl => _serverUrl;

        public string ClientType => _clientType;

        public Credentials Credentials => _credentials;

        public IDictionary<string, string> Cookies => _cookies;

        public string XsrfTokenValue => _xsrfTokenValue;

        public ILogger Logger => _logger;

        public string BuildRestEndpoint(string suffix) => _restPrefix.AppendSuffix(suffix);

        public string BuildWebUIEndpoint(string suffix) => _webuiPrefix.AppendSuffix(suffix);

        public async Task<Response> HttpGet(string url, WebHeaderCollection headers = null, ResourceAccessLevel resourceAccessLevel = ResourceAccessLevel.PUBLIC, string query = "", bool logRequestUrl = true)
        {
            Response res = null;
            using (var client = new WebClient { Headers = headers })
            {
                try
                {
                    if (!query.IsNullOrWhiteSpace())
                        url += $"?{query}";

                    if (logRequestUrl)
                    {
                        await _logger.LogDebug($"GET {url}");
                    }

                    DecorateRequestHeaders(client, resourceAccessLevel);
                    string data = await client.DownloadStringTaskAsync(url);
                    //PrintHeaders(client);

                    res = new Response(data, client.ResponseHeaders, HttpStatusCode.OK);
                    UpdateCookies(client);
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (WebException we)
                {
                    await _logger.LogError(we.Message);
                    //PrintHeaders(client);
                    if (we.Response is HttpWebResponse resp)
                        return new Response(we.Message, resp.StatusCode);
                    else
                        return new Response(we.Message);
                }
                catch (Exception e)
                {
                    await _logger.LogError(e.Message);
                    //PrintHeaders(client);
                    res = new Response(e.Message);
                }
            }
            return res;
        }

        public async Task<Response> HttpPost(string url, WebHeaderCollection headers = null, string body = null, ResourceAccessLevel resourceAccessLevel = ResourceAccessLevel.PUBLIC, bool logRequestUrl = true)
        {
            Response res;
            using var client = new WebClient { Headers = headers };
            try
            {
                if (logRequestUrl)
                {
                    await _logger.LogDebug($"POST {url}");
                }
                DecorateRequestHeaders(client, resourceAccessLevel);
                string data = await client.UploadStringTaskAsync(url, body);
                //PrintHeaders(client);

                res = new Response(data, client.ResponseHeaders, HttpStatusCode.OK);
                UpdateCookies(client);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (WebException we)
            {
                await _logger.LogError(we.Message);
                //PrintHeaders(client);
                if (we.Response is HttpWebResponse resp)
                    return new Response(we.Message, resp.StatusCode);
                else
                    return new Response(we.Message);
            }
            catch (Exception e)
            {
                await _logger.LogError(e.Message);
                //PrintHeaders(client);
                res = new Response(e.Message);
            }
            return res;
        }

        public async Task<Response> HttpPut(string url, WebHeaderCollection headers = null, string body = null, ResourceAccessLevel resourceAccessLevel = ResourceAccessLevel.PUBLIC)
        {
            // NOT implemented 
            return await Task.FromResult(new Response());
        }

        private void DecorateRequestHeaders(WebClient client, ResourceAccessLevel resourceAccessLevel)
        {
            //attach encrypted user name for protected and private resources
            if (resourceAccessLevel.In(ResourceAccessLevel.PROTECTED, ResourceAccessLevel.PRIVATE))
            {
                string userHeaderName = resourceAccessLevel.GetStringValue();
                if (userHeaderName != null)
                    client.Headers.Add(userHeaderName, _credentials.UsernameOrClientID.GetMD5Hash());
            }
            if (_cookies.Any())
                client.Headers.Add(HttpRequestHeader.Cookie, _rawCookies);
        }

        private string GetCookiesAsString()
        {
            var sb = new StringBuilder();
            if (_cookies.Any())
            {
                foreach (KeyValuePair<string, string> cookie in _cookies)
                {
                    sb.Append($"{cookie.Key}={cookie.Value};");
                }
            }
            return sb.ToString();
        }
        private void UpdateCookies(WebClient client)
        {
            string[] newCookies = client.ResponseHeaders?.GetValues(SET_COOKIE);
            if (newCookies != null)
            {
                foreach (string cookie in newCookies)
                {
                    int equalIdx = cookie.IndexOf(C.EQUAL);
                    int semicolonIndex = cookie.IndexOf(C.SEMICOLON);
                    string key = cookie.Substring(0, equalIdx);
                    string val = cookie.Substring(equalIdx + 1, semicolonIndex - equalIdx - 1);
                    if (_cookies.ContainsKey(key))
                        _cookies[key] = val;
                    else
                        _cookies.Add(key, val);
                    //_logger.LogInfo($"{key} = {val}");
                }
            }
        }

        private void PrintHeaders(WebClient client)
        {
            var headers = client.Headers;
            var keys = headers.AllKeys;
            _logger.LogDebug("Headers:");
            foreach (string key in keys)
            {
                _logger.LogDebug($"{key} = {headers[key]}");
            }
            if (client.ResponseHeaders != null)
            {
                _logger.LogDebug("ResponseHeaders:");
                headers = client.ResponseHeaders;
                keys = headers.AllKeys;
                foreach (string key in keys)
                {
                    _logger.LogDebug($"{key} = {headers[key]}");
                }
            }
        }
    }
}

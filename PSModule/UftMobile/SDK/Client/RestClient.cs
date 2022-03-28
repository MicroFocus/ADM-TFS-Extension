using PSModule.UftMobile.SDK.Interface;
using PSModule.UftMobile.SDK.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSModule.UftMobile.SDK
{
    using C = Constants;
    public class RestClient : IClient
    {
        private const string X_HP4MSECRET = "x-hp4msecret";
        private const string JSESSIONID = "JSESSIONID";
        private const string SET_COOKIE = "Set-Cookie";

        protected readonly Uri _serverUrl; // Example : http://myd-vm21045.swinfra.net:8080/qcbin
        protected IDictionary<string, string> _cookies = new Dictionary<string, string>();
        private readonly Credentials _credentials;
        private readonly string _xsrfTokenValue;
        private readonly ILogger _logger;
        private string _rawCookies => GetCookiesAsString();
        private string _hp4msecret;

        static RestClient()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public RestClient(string serverUrl, Credentials credentials, ILogger logger)
        {
            _serverUrl = new Uri(serverUrl);
            _credentials = credentials;
            _logger = logger;
            _xsrfTokenValue = Guid.NewGuid().ToString();
            //_cookies.Add(XSRF_TOKEN, _xsrfTokenValue);
        }

        public Uri ServerUrl => _serverUrl;

        public Credentials Credentials => _credentials;

        public IDictionary<string, string> Cookies => _cookies;

        public string XsrfTokenValue => _xsrfTokenValue;

        public ILogger Logger => _logger;

        public async Task<Response<T>> HttpGet<T>(string url, WebHeaderCollection headers = null, string query = "", bool logError = true)
        {
            Response<T> res = null;
            if (headers == null && _cookies.Any() && !_hp4msecret.IsNullOrWhiteSpace())
                headers = new WebHeaderCollection
                {
                    { HttpRequestHeader.Cookie, $"{JSESSIONID}={_cookies[JSESSIONID]}" },
                    { X_HP4MSECRET, _hp4msecret }
                };

            using (var client = new WebClient { Headers = headers })
            {
                try
                {
                    if (!query.IsNullOrWhiteSpace())
                        url += $"?{query}";

                    await _logger.LogDebug($"GET {url}");

                    DecorateRequestHeaders(client);
                    string data = await client.DownloadStringTaskAsync(url);
                    await _logger.LogDebug($"{data}");
                    if (_logger.IsDebug)
                        PrintHeaders(client);

                    res = new Response<T>(data, client.ResponseHeaders, HttpStatusCode.OK);
                    UpdateCookies(client);
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (WebException we)
                {
                    if (logError || _logger.IsDebug)
                        await _logger.LogError(we.Message);
                    if (_logger.IsDebug)
                        PrintHeaders(client);
                    if (we.Response is HttpWebResponse resp)
                        return new Response<T>(we.Message, resp.StatusCode);
                    else
                        return new Response<T>(we.Message);
                }
                catch (Exception e)
                {
                    if (logError || _logger.IsDebug)
                        await _logger.LogError(e.Message);
                    if (_logger.IsDebug)
                        PrintHeaders(client);
                    res = new Response<T>(e.Message);
                }
            }
            return res;
        }

        public async Task<Response> HttpPost(string url, string body, WebHeaderCollection headers = null)
        {
            Response res;
            if (headers == null)
            {
                headers = new WebHeaderCollection
                {
                    { HttpRequestHeader.Accept, C.APP_JSON },
                    { HttpRequestHeader.ContentType, C.APP_JSON_UTF8 }
                };
                if (_cookies.Any() && !_hp4msecret.IsNullOrWhiteSpace())
                {
                    headers.Add(X_HP4MSECRET, _hp4msecret);
                }
            }
            using var client = new WebClient { Headers = headers };
            try
            {
                await _logger.LogDebug($"POST {url}");
                DecorateRequestHeaders(client);
                string data = await client.UploadStringTaskAsync(url, body);
                if (_logger.IsDebug)
                    PrintHeaders(client);

                res = new Response(data, client.ResponseHeaders, HttpStatusCode.OK);
                UpdateCookies(client);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (WebException we)
            {
                if (_logger.IsDebug)
                {
                    await _logger.LogError(we.Message);
                    PrintHeaders(client);
                }
                if (we.Response is HttpWebResponse resp)
                    return new Response(we.Message, resp.StatusCode);
                else
                    return new Response(we.Message);
            }
            catch (Exception e)
            {
                if (_logger.IsDebug)
                {
                    await _logger.LogError(e.Message);
                    PrintHeaders(client);
                }
                res = new Response(e.Message);
            }
            return res;
        }

        public async Task<Response<T>> HttpPost<T>(string url, string body, WebHeaderCollection headers = null)
        {
            Response<T> res;
            if (headers == null)
            {
                headers = new WebHeaderCollection
                {
                    { HttpRequestHeader.Accept, C.APP_JSON },
                    { HttpRequestHeader.ContentType, C.APP_JSON_UTF8 }
                };
                if (_cookies.Any() && !_hp4msecret.IsNullOrWhiteSpace())
                {
                    //headers.Add(JSESSIONID, _cookies[JSESSIONID]);
                    headers.Add(X_HP4MSECRET, _hp4msecret);
                }
            }

            using var client = new WebClient { Headers = headers };
            try
            {
                await _logger.LogDebug($"POST {url}");

                DecorateRequestHeaders(client);
                string data = await client.UploadStringTaskAsync(url, body);
                if (_logger.IsDebug)
                    PrintHeaders(client);

                res = new Response<T>(data, client.ResponseHeaders, HttpStatusCode.OK);
                UpdateCookies(client);
            }
            catch (WebException we)
            {
                if (_logger.IsDebug)
                    PrintHeaders(client);
                if (we.Response is HttpWebResponse resp)
                    return new Response<T>(we.Message, resp.StatusCode);
                else
                    return new Response<T>(we.Message);
            }
            catch (Exception e)
            {
                if (_logger.IsDebug)
                    PrintHeaders(client);
                res = new Response<T>(e.Message);
            }

            return res;
        }

        public Task<Response> HttpPut(string url, WebHeaderCollection headers = null, string body = null)
        {
            throw new NotImplementedException();
        }

        private void DecorateRequestHeaders(WebClient client)
        {
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
                if (client.ResponseHeaders.AllKeys.Contains(X_HP4MSECRET))
                    _hp4msecret = client.ResponseHeaders[X_HP4MSECRET];
            }
        }

        private void PrintHeaders(WebClient client)
        {
            var headers = client.Headers;
            var keys = headers.AllKeys;
            _logger.LogDebug("Request Headers:");
            foreach (string key in keys)
            {
                _logger.LogDebug($"{key} = {headers[key]}");
            }
            if (client.ResponseHeaders != null)
            {
                _logger.LogDebug("Response Headers:");
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

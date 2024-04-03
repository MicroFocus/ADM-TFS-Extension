/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2024 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

using PSModule.Common;
using PSModule.Properties;
using PSModule.UftMobile.SDK.Entity;
using PSModule.UftMobile.SDK.Enums;
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
    public class RestClient(string serverUrl, Credentials credentials, ILogger logger, AuthType authType, IWebProxy proxy) : IClient
    {
        private const string X_HP4MSECRET = "x-hp4msecret";
        private const string JSESSIONID = "JSESSIONID";
        private const string SET_COOKIE = "Set-Cookie";
        private const string PUT = "PUT";

        private readonly Uri _serverUrl = new(serverUrl); // Example : http://myd-vm21045.swinfra.net:8080/qcbin
        private readonly Dictionary<string, string> _cookies = [];
        private readonly Credentials _credentials = credentials;
        private readonly ILogger _logger = logger;
        private string _rawCookies => GetCookiesAsString();
        private string _hp4msecret;
        private readonly AuthType _authType = authType;
        private readonly IWebProxy _proxy = proxy;
        private AccessToken _accessToken;
        private bool _isLoggedIn;

        static RestClient()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public Uri ServerUrl => _serverUrl;

        public Credentials Credentials => _credentials;

        public ILogger Logger => _logger;
        public AuthType AuthType => _authType;
        public bool IsLoggedIn { get { return _isLoggedIn; } set { _isLoggedIn = value; } }
        public AccessToken AccessToken { get { return _accessToken; } set { _accessToken = value; } }

        public async Task<Response<T>> HttpGet<T>(string endpoint, WebHeaderCollection headers = null, string query = "", bool logError = true, ResType resType = ResType.DataEntities) where T : class
        {
            Response<T> res = null;
            if (!TryBuildHeaders(ref headers, out string err))
            {
                if (logError || _logger.IsDebug)
                    await _logger.LogError(err);
                return new(err);
            }

            using WebClient client = new() { Headers = headers };
            try
            {
                if (_proxy != null)
                    client.Proxy = _proxy;

                if (!query.IsNullOrWhiteSpace())
                    endpoint += $"?{query}";

                await _logger.LogDebug($"GET {endpoint}");

                DecorateRequestHeaders(client);
                string data = await client.DownloadStringTaskAsync(ServerUrl.AppendSuffix(endpoint));
                await _logger.LogDebug($"{data}");
                if (_logger.IsDebug)
                    PrintHeaders(client);

                res = new(data, client.ResponseHeaders, HttpStatusCode.OK, resType);
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
                    return new(we.Message, resp.StatusCode);
                else
                    return new(we.Message);
            }
            catch (Exception e)
            {
                if (logError || _logger.IsDebug)
                    await _logger.LogError(e.Message);
                if (_logger.IsDebug)
                    PrintHeaders(client);
                res = new(e.Message);
            }
            return res;
        }

        public async Task<Response> HttpPost(string endpoint, string body, WebHeaderCollection headers = null)
        {
            Response res;
            if (!TryBuildHeaders(ref headers, out string err))
            {
                if (_logger.IsDebug)
                    await _logger.LogError(err);
                return new(err);
            }
            using WebClient client = new() { Headers = headers };
            try
            {
                if (_proxy != null)
                    client.Proxy = _proxy;

                await _logger.LogDebug($"POST {endpoint}");
                DecorateRequestHeaders(client);
                string data = await client.UploadStringTaskAsync(ServerUrl.AppendSuffix(endpoint), body);
                if (_logger.IsDebug)
                    PrintHeaders(client);

                res = new(data, client.ResponseHeaders, HttpStatusCode.OK);
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
                    PrintHeaders(client);
                    await _logger.LogDebug(body);
                }
                if (we.Response is HttpWebResponse resp)
                    return new(we.Message, resp.StatusCode);
                else
                    return new(we.Message);
            }
            catch (Exception e)
            {
                if (_logger.IsDebug)
                {
                    PrintHeaders(client);
                    await _logger.LogDebug(body);
                }
                res = new(e.Message);
            }
            return res;
        }

        public async Task<Response<T>> HttpPost<T>(string endpoint, string body, WebHeaderCollection headers = null, ResType resType = ResType.Object) where T : class
        {
            Response<T> res;
            if (!TryBuildHeaders(ref headers, out string err))
            {
                if (_logger.IsDebug)
                    await _logger.LogError(err);
                return new(err);
            }

            using WebClient client = new() { Headers = headers };
            try
            {
                if (_proxy != null)
                    client.Proxy = _proxy;

                await _logger.LogDebug($"POST {endpoint}");

                DecorateRequestHeaders(client);
                string data = await client.UploadStringTaskAsync(ServerUrl.AppendSuffix(endpoint), body);
                if (_logger.IsDebug)
                    PrintHeaders(client);

                res = new(data, client.ResponseHeaders, HttpStatusCode.OK, resType);
                UpdateCookies(client);
            }
            catch (WebException we)
            {
                if (_logger.IsDebug)
                {
                    PrintHeaders(client);
                    await _logger.LogDebug(body);
                }
                if (we.Response is HttpWebResponse resp)
                    return new(we.Message, resp.StatusCode);
                else
                    return new(we.Message);
            }
            catch (Exception e)
            {
                if (_logger.IsDebug)
                {
                    PrintHeaders(client);
                    await _logger.LogDebug(body);
                }
                res = new(e.Message);
            }

            return res;
        }

        public async Task<Response> HttpPut(string endpoint, string body, WebHeaderCollection headers = null)
        {
            Response res;
            if (!TryBuildHeaders(ref headers, out string err))
            {
                if (_logger.IsDebug)
                    await _logger.LogError(err);
                return new(err);
            }
            using WebClient client = new() { Headers = headers };
            try
            {
                if (_proxy != null)
                    client.Proxy = _proxy;

                await _logger.LogDebug($"PUT {endpoint}");
                DecorateRequestHeaders(client);
                string data = await client.UploadStringTaskAsync(ServerUrl.AppendSuffix(endpoint), PUT, body);
                if (_logger.IsDebug)
                    PrintHeaders(client);

                res = new(data, client.ResponseHeaders, HttpStatusCode.OK);
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
                    PrintHeaders(client);
                    await _logger.LogDebug(body);
                }
                if (we.Response is HttpWebResponse resp)
                    return new(we.Message, resp.StatusCode);
                else
                    return new(we.Message);
            }
            catch (Exception e)
            {
                if (_logger.IsDebug)
                {
                    PrintHeaders(client);
                    await _logger.LogDebug(body);
                }
                res = new(e.Message);
            }
            return res;
        }

        private void DecorateRequestHeaders(WebClient client)
        {
            if (_cookies.Any())
                client.Headers.Add(HttpRequestHeader.Cookie, _rawCookies);
        }

        private string GetCookiesAsString()
        {
            StringBuilder sb = new();
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

        private bool TryBuildHeaders(ref WebHeaderCollection headers, out string err)
        {
            err = string.Empty;
            bool ok = false;
            headers ??= new()
                {
                    { HttpRequestHeader.Accept, C.APP_JSON },
                    { HttpRequestHeader.ContentType, C.APP_JSON_UTF8 }
                };

            if (_isLoggedIn)
            {
                headers.Add(X_HP4MSECRET, _hp4msecret);
                if (_authType == AuthType.Basic)
                {
                    headers.Add(HttpRequestHeader.Cookie, $"{JSESSIONID}={_cookies[JSESSIONID]}");
                    ok = true;
                }
                else if (_authType == AuthType.AccessKey)
                {
                    if (_accessToken == null)
                    {
                        err = Resources.AccessTokenIsNull;
                    }
                    else if (_accessToken.IsExpired())
                    {
                        err = Resources.AccessTokenExpired;
                    }
                    else
                    {
                        headers.Add(HttpRequestHeader.Authorization, $"Bearer {_accessToken.Value}");
                        //TODO add tenant-id header, required when using shared spaces (multitenancy) with Digital Lab
                        ok = true;
                    }
                }
                else
                {
                    err = Resources.AuthTypeIsInvalid;
                }
            }
            else
            {
                ok = true;
            }
            return ok;
        }
    }
}

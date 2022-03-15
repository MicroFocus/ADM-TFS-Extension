using PSModule.UftMobile.SDK.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PSModule.UftMobile.SDK.Interface
{
    public interface IClient
    {
        Task<Response<T>> HttpGet<T>(
                string url,
                WebHeaderCollection headers = null,
                string query = "",
                bool logRequestUrl = true,
                bool logError = true);

        Task<Response> HttpPost(
                string url,
                WebHeaderCollection headers = null,
                string body = null,
                bool logRequestUrl = true);

        Task<Response> HttpPut(
                string url,
                WebHeaderCollection headers = null,
                string body = null);

        Uri ServerUrl { get; }

        Credentials Credentials { get; }

        IDictionary<string, string> Cookies { get; }

        string XsrfTokenValue { get; }

        ILogger Logger { get; }
    }
}

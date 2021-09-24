using PSModule.AlmLabMgmtClient.SDK.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Interface
{
    public interface IClient
    {
        Task<Response> HttpGet(
                string url,
                WebHeaderCollection headers = null,
                ResourceAccessLevel resourceAccessLevel = ResourceAccessLevel.PUBLIC,
                string query = "",
                bool logUrl = true);

        Task<Response> HttpPost(
                string url,
                WebHeaderCollection headers = null,
                string body = null,
                ResourceAccessLevel resourceAccessLevel =  ResourceAccessLevel.PUBLIC,
                bool logUrl = true);

        Task<Response> HttpPut(
                string url,
                WebHeaderCollection headers = null,
                string body = null,
                ResourceAccessLevel resourceAccessLevel = ResourceAccessLevel.PUBLIC);

        string BuildRestEndpoint(string suffix);

        string BuildWebUIEndpoint(string suffix);

        Uri ServerUrl { get; }

        string ClientType { get; }
        Credentials Credentials { get; }

        IDictionary<string, string> Cookies { get; }

        string XsrfTokenValue { get; }

        ILogger Logger { get; }
    }
}

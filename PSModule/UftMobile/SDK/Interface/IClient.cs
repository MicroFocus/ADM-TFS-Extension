using PSModule.UftMobile.SDK.Entity;
using PSModule.UftMobile.SDK.Enums;
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
                string endpoint,
                WebHeaderCollection headers = null,
                string query = "",
                bool logError = true,
                ResType resType = ResType.DataEntities) where T : class;

        Task<Response> HttpPost(
                string endpoint,
                string body,
                WebHeaderCollection headers = null);
        Task<Response<T>> HttpPost<T>(
                string endpoint,
                string body,
                WebHeaderCollection headers = null,
                ResType resType = ResType.Object) where T : class;

        Task<Response> HttpPut(
                string endpoint,
                string body,
                WebHeaderCollection headers = null);

        Uri ServerUrl { get; }

        Credentials Credentials { get; }

        IDictionary<string, string> Cookies { get; }

        ILogger Logger { get; }

        bool IsLoggedIn { set; get; }
        AccessToken AccessToken { get; set; }
    }
}

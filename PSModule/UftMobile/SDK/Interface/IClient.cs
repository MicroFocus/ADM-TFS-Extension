/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2023 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

using PSModule.UftMobile.SDK.Entity;
using PSModule.UftMobile.SDK.Enums;
using PSModule.UftMobile.SDK.Util;
using System;
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

        ILogger Logger { get; }

        bool IsLoggedIn { set; get; }
        AccessToken AccessToken { get; set; }
    }
}

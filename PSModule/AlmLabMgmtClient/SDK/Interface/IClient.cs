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
                bool logUrl = true,
                bool logError = true);

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

        string XsrfTokenValue { get; }

        ILogger Logger { get; }
    }
}

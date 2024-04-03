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

using PSModule.AlmLabMgmtClient.SDK.Interface;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Request
{
    public abstract class RequestBase : IRequest
    {
        protected readonly IClient _client;
        protected readonly ILogger _logger;
        protected const string X_XSRF_TOKEN = "X-XSRF-TOKEN";
        protected const string PROC_RUNS = "procedure-runs";

        protected RequestBase(IClient client)
        {
            _client = client;
            _logger = _client.Logger;
        }

        public async Task<Response> Execute(bool logRequestUrl = true)
        {
            Response res;
            try
            {
                res = await Perform(logRequestUrl);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex.Message);
                res = new Response(ex.Message);
            }

            return res;
        }

        public abstract Task<Response> Perform(bool logRequestUrl = true);

        protected virtual string Suffix => null;

        protected virtual WebHeaderCollection Headers => new WebHeaderCollection { { X_XSRF_TOKEN, _client.XsrfTokenValue } };

        protected string Body => null;

        protected virtual string Url => _client.BuildRestEndpoint(Suffix);
    }
}
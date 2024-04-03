/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2024 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 *
 * 
 */

using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Util;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Handler
{
    public abstract class PollHandler : HandlerBase
    {
        private readonly int _interval = 5000; // millisecond
        protected PollHandler(IClient client, string entityId) : base(client, entityId)
        {
        }

        protected PollHandler(IClient client, string entityId, int interval) : base(client, entityId)
        {
            // NOTE: this constructor is not used at this moment, but for safety, restrict the polling interval between 1 and 60 seconds
            if (interval < 1000 || interval > 60000)
                throw new AlmException($"PollHandler: Invalid polling interval : {interval} milliseconds. Between 1000 and 60000 milliseconds", ErrorCategory.LimitsExceeded);
            _interval = interval;
        }

        protected PollHandler(IClient client, string entityId, string runId) : base(client, entityId, runId)
        {
        }

        public async Task<bool> Poll()
        {
            await _logger.LogInfo($"Start Polling... Run ID: {_runId}");
            return await DoPoll();
        }

        protected virtual async Task<bool> DoPoll()
        {
            bool ok = false, logRequestUrl = true;

            int failures = 0;
            while (failures < 3)
            {
                var res = await GetResponse(logRequestUrl);
                if (res.IsOK)
                {
                    await LogProgress(logRequestUrl);
                    if (IsFinished(res))
                    {
                        ok = true;
                        LogRunEntityResults(await GetRunEntityResultsResponse());
                        break;
                    }
                    else
                    {
                        if (logRequestUrl)
                            await _logger.ShowProgress();
                        logRequestUrl = false;
                    }
                }
                else
                {
                    LogPollingError(res);
                    ++failures;
                }
                Sleep();
            }

            return ok;
        }

        protected abstract Task<Response> GetRunEntityResultsResponse();

        protected abstract bool LogRunEntityResults(Response response);

        protected abstract bool IsFinished(Response response);

        protected abstract Task<Response> GetResponse(bool logRequestUrl);

        protected void Sleep()
        {
            try
            {
                Thread.Sleep(_interval);
            }
            catch (ThreadInterruptedException)
            {
                _logger.LogError("Interrupted while polling");
                throw;
            }
        }

        protected void LogPollingError(Response res)
        {
            string err = "Polling try failed. ";
            if (res.StatusCode != null)
            {
                err += $"Status code: {res.StatusCode}, ";
            }
            err += $"Error: {res.Error ?? "Not Available"}";
            _logger.LogError(err);
        }

        protected abstract Task LogProgress(bool logRequestUrl);

    }
}
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
using PSModule.AlmLabMgmtClient.SDK.Request;
using PSModule.AlmLabMgmtClient.SDK.Util;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Handler
{
    public abstract class RunHandler : HandlerBase
    {
        protected abstract string StartSuffix { get; }

        public abstract string NameSuffix { get; }

        protected RunHandler(IClient client, string entityId) : base(client, entityId) { }

        public async Task<Response> Start(string duration, string envConfigId)
        {
            return await new StartRunEntityRequest(_client, StartSuffix, _entityId, duration, envConfigId).Execute();
        }

        public async Task<Response> Stop()
        {
            return await new StopEntityRequest(_client, _runId).Execute();
        }

        public async Task<string> ReportUrl(Args args)
        {
            return await new AlmRunReportUrlBuilder().Build(_client, args.Domain, args.Project, _runId);
        }

        public RunResponse GetRunResponse(Response response)
        {
            RunResponse res = new RunResponse();
            res.Initialize(response);

            return res;
        }
    }
}

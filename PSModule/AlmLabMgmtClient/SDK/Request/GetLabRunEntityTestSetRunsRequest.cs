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
using PSModule.Common;
using System.Net;

namespace PSModule.AlmLabMgmtClient.SDK.Request
{
    using C = Constants;
    public class GetLabRunEntityTestSetRunsRequest : GetRequest
    {
        private const string PROCEDURE_TESTSET_INSTANCE_RUNS = "procedure-testset-instance-runs";
        public GetLabRunEntityTestSetRunsRequest(IClient client, string runId) : base(client, runId)
        {
        }

        protected override string Suffix => PROCEDURE_TESTSET_INSTANCE_RUNS;

        protected override string QueryString => $"query={{procedure-run[{_runId}]}}&page-size=2000&fields=test-subtype,start-exec-time,test-config-name,duration,start-exec-date,testset-name,testcycl-id,status,run-id";

        // It's pretty weird that in 1260 p1 the xml header should be provided. Otherwise the server would generate wrong query sql.
        protected override WebHeaderCollection Headers =>
            new()
            {
                { HttpRequestHeader.ContentType, C.APP_XML },
                { HttpRequestHeader.Accept, C.APP_XML }
            };

}
}
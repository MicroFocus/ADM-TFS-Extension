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
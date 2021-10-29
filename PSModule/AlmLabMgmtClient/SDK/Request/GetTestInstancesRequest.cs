using PSModule.AlmLabMgmtClient.SDK.Interface;

namespace PSModule.AlmLabMgmtClient.SDK.Request
{
    public class GetTestInstancesRequest : GetRequestBase
    {
        private const string TEST_INSTANCES = "test-instances";
        private readonly string _testsetId;

        public GetTestInstancesRequest(IClient client, string testsetId) : base(client)
        {
            _testsetId = testsetId;
        }
        protected override string Suffix => TEST_INSTANCES;
        protected override string QueryString => $"query={{cycle-id[{_testsetId}]}}&fields=id";
    }
}

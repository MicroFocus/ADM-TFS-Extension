using PSModule.AlmLabMgmtClient.SDK.Interface;

namespace PSModule.AlmLabMgmtClient.SDK.Request
{
    public class GetTestsetRequest : GetRequestBase
    {
        private const string TEST_SETS = "test-sets";
        private readonly string _testsetId;

        public GetTestsetRequest(IClient client, string testsetId) : base(client)
        {
            _testsetId = testsetId;
        }
        protected override string Suffix => TEST_SETS;
        protected override string QueryString => $"query={{id[{_testsetId}]}}&fields=id,name,subtype-id";
    }
}

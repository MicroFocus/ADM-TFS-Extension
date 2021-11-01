using PSModule.AlmLabMgmtClient.SDK.Interface;

namespace PSModule.AlmLabMgmtClient.SDK.Request
{
    public class GetTestSetRequest : GetRequestBase
    {
        private const string TEST_SETS = "test-sets";
        private readonly string _testSetId;

        public GetTestSetRequest(IClient client, string testSetId) : base(client)
        {
            _testSetId = testSetId;
        }

        protected override string Suffix => TEST_SETS;
        protected override string QueryString => @$"query={{id[{_testSetId}];subtype-id[""hp.sse.test-set.process""]}}&fields=id,name&page-size=1";
    }
}

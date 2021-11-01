using PSModule.AlmLabMgmtClient.SDK.Interface;
using System.Collections.Generic;

namespace PSModule.AlmLabMgmtClient.SDK.Request
{
    public class GetTestInstancesRequest : GetRequestBase
    {
        private const string TEST_INSTANCES = "test-instances";
        private const string OR = " OR ";
        private readonly string _testSetIds;

        public GetTestInstancesRequest(IClient client, string testsetId) : base(client)
        {
            _testSetIds = testsetId;
        }
        public GetTestInstancesRequest(IClient client, IList<int> testsetIds) : base(client)
        {
            _testSetIds = string.Join(OR, testsetIds);
        }
        protected override string Suffix => TEST_INSTANCES;
        protected override string QueryString => $"query={{cycle-id[{_testSetIds}]}}&fields=cycle-id&page-size=max";
    }
}

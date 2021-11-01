using PSModule.AlmLabMgmtClient.SDK.Interface;

namespace PSModule.AlmLabMgmtClient.SDK.Request
{
    public class GetBvsTestSetsRequest : GetRequestBase
    {
        private const string PROCEDURE_TESTSETS = "procedure-testsets";
        private readonly string _bvsId;

        public GetBvsTestSetsRequest(IClient client, string bvsId) : base(client)
        {
            _bvsId = bvsId;
        }

        protected override string Suffix => PROCEDURE_TESTSETS;
        protected override string QueryString => $"query={{parent-id[{_bvsId}]}}&fields=cycle-id&page-size=max";
    }
}

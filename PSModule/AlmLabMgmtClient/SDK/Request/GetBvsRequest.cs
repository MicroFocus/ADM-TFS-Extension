using PSModule.AlmLabMgmtClient.SDK.Interface;

namespace PSModule.AlmLabMgmtClient.SDK.Request
{
    public class GetBvsRequest : GetRequestBase
    {
        private readonly string _bvsId;
        private const string PROCEDURES = "procedures";

        public GetBvsRequest(IClient client, string bvsId) : base(client)
        {
            _bvsId = bvsId;
        }

        protected override string Suffix => PROCEDURES;
        protected override string QueryString => $"query={{id[{_bvsId}]}}&fields=id,name&page-size=1";
    }
}

using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.Common;
using System.Net;

namespace PSModule.AlmLabMgmtClient.SDK.Request
{
    using C = Constants;
    public class GetAlmVersionRequest : GetRequest
    {
        private const string REST_SA_VERSION = "rest/sa/version";
        public GetAlmVersionRequest(IClient client) : base(client, null) { }

        protected override string Suffix => REST_SA_VERSION;

        protected override string Url => _client.ServerUrl.AppendSuffix(Suffix);

        protected override WebHeaderCollection Headers => 
            new()
            {
                { HttpRequestHeader.ContentType, C.APP_XML },
                { HttpRequestHeader.Accept, C.APP_XML }
            };
    }
}
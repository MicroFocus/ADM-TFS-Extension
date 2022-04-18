using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Util;
using PSModule.Common;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Request
{
    using C = Constants;
    public abstract class PostRequestBase : RequestBase
    {
        protected PostRequestBase(IClient client) : base(client) { }

        protected override WebHeaderCollection Headers =>
            new()
            {
                { HttpRequestHeader.ContentType, C.APP_XML },
                { HttpRequestHeader.Accept, C.APP_XML },
                { X_XSRF_TOKEN, _client.XsrfTokenValue }
            };

        public async override Task<Response> Perform(bool logRequestUrl = true)
        {
            return await _client.HttpPost(
                    Url,
                    Headers,
                    GetXmlData(),
                    ResourceAccessLevel.PROTECTED,
                    logRequestUrl);
        }

        private string GetXmlData()
        {
            StringBuilder builder = new StringBuilder("<Entity><Fields>");
            foreach (KeyValuePair<string, string> pair in DataFields)
            {
                builder.Append($"<Field Name=\"{pair.Key}\"><Value>{pair.Value}</Value></Field>");
            }
            return builder.Append("</Fields></Entity>").ToString();
        }

        protected virtual IList<KeyValuePair<string, string>> DataFields => new List<KeyValuePair<string, string>>();
    }
}
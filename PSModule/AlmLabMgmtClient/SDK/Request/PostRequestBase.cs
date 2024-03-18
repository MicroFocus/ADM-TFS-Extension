/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2023 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

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
            StringBuilder builder = new("<Entity><Fields>");
            foreach (KeyValuePair<string, string> pair in DataFields)
            {
                builder.Append($"<Field Name=\"{pair.Key}\"><Value>{pair.Value}</Value></Field>");
            }
            return builder.Append("</Fields></Entity>").ToString();
        }

        protected virtual IList<KeyValuePair<string, string>> DataFields => [];
    }
}
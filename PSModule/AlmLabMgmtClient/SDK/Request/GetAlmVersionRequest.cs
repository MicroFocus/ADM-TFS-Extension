/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2024 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

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
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
using PSModule.AlmLabMgmtClient.SDK.Request;

namespace AlmLabMgmtClient.SDK.Request
{
    public class GetRunEntityNameRequest : GetRequest
    {
        private readonly string _nameSuffix;

        public GetRunEntityNameRequest(IClient client, string suffix, string entityId) : base(client, entityId)
        {
            _nameSuffix = suffix;
        }

        protected override string Suffix => _nameSuffix;
    }
}
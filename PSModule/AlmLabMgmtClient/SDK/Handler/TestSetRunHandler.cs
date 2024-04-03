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

namespace PSModule.AlmLabMgmtClient.SDK.Handler
{
    public class TestSetRunHandler : RunHandler
    {
        public TestSetRunHandler(IClient client, string entityId) : base(client, entityId) { }

        protected override string StartSuffix => $"test-sets/{_entityId}/startruntestset";

        public override string NameSuffix => $"test-sets/{_entityId}";
    }
}

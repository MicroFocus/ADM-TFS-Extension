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

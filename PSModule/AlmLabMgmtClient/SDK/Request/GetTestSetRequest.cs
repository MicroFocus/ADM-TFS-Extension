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

namespace PSModule.AlmLabMgmtClient.SDK.Request
{
    public class GetTestSetRequest : GetRequestBase
    {
        private const string TEST_SETS = "test-sets";
        private readonly string _testSetId;

        public GetTestSetRequest(IClient client, string testSetId) : base(client)
        {
            _testSetId = testSetId;
        }

        protected override string Suffix => TEST_SETS;
        protected override string QueryString => @$"query={{id[{_testSetId}];subtype-id[""hp.sse.test-set.process""]}}&fields=id,name&page-size=1";
    }
}

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

using PSModule.AlmLabMgmtClient.SDK.Util;
using PSModule.Common;

namespace PSModule.AlmLabMgmtClient.SDK
{
    using C = Constants;
    public class RunResponse
    {
        private const string SUCCESS_STATUS = "SuccessStaus";
        private const string INFO = "info";

        private string _successStatus;
        private string _runId;

        public void Initialize(Response response)
        {
            string xml = response.ToString();
            _successStatus = Xml.GetAttributeValue(xml, SUCCESS_STATUS);
            _runId = ParseRunId(Xml.GetAttributeValue(xml, INFO));
        }

        protected string ParseRunId(string runIdResponse)
        {
            return runIdResponse.IsNullOrWhiteSpace() ? C.NO_RUN_ID : runIdResponse;
        }

        public string RunId => _runId;

        public bool HasSucceeded => _successStatus == C.ONE;

    }
}
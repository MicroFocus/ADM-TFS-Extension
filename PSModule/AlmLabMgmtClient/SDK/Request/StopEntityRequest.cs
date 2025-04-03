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
    public class StopEntityRequest : PostRequest
    {
        private const string STOP = "stop";
        private const string SKIP_DEPROV = "skipDeprovision";
        public StopEntityRequest(IClient client, string runId) : base(client, runId) { }
        protected override string Suffix => $"{PROC_RUNS}/{_runId}/{STOP}";
        protected override IList<KeyValuePair<string, string>> DataFields => GetDataFields();
        private IList<KeyValuePair<string, string>> GetDataFields()
        {
            List<KeyValuePair<string, string>> fields =
            [
                new(SKIP_DEPROV, bool.TrueString.ToLower())
            ];

            return fields;
        }
    }
}
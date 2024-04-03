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
    public class StartRunEntityRequest : PostRequest
    {
        private const string DURATION = "duration";
        private const string VUDS_MODE = "vudsMode";
        private const string RESERVATION_ID = "reservationId";
        private const string MINUS_ONE = "-1";
        private const string VALUE_SET_ID = "valueSetId";

        private readonly string _duration;
        private readonly string _suffix;
        private readonly string _envConfigId;

        public StartRunEntityRequest(IClient client, string suffix, string runId, string duration, string envConfigId) : base(client, runId)
        {
            _duration = duration;
            _suffix = suffix;
            _envConfigId = envConfigId;
        }

        protected override IList<KeyValuePair<string, string>> DataFields => GetDataFields();

        private IList<KeyValuePair<string, string>> GetDataFields()
        {
            List<KeyValuePair<string, string>> fields =
            [
                new(DURATION, _duration),
                new(VUDS_MODE, bool.FalseString.ToLower()),
                new(RESERVATION_ID, MINUS_ONE)
            ];
            if (!_envConfigId.IsNullOrWhiteSpace())
            {
                fields.Add(new(VALUE_SET_ID, _envConfigId));
            }

            return fields;
        }

        protected override string Suffix => _suffix;

    }
}
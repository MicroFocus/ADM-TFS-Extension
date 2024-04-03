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
    public abstract class HandlerBase
    {
        protected readonly IClient _client;
        protected readonly ILogger _logger;
        protected readonly string _entityId;
        protected string _runId = string.Empty;
        protected string _timeslotId = string.Empty;

        protected HandlerBase(IClient client, string entityId)
        {
            _client = client;
            _entityId = entityId;
            _logger = _client.Logger;
        }

        protected HandlerBase(IClient client, string entityId, string runId) : this(client, entityId)
        {
            _runId = runId;
        }

        public string EntityId => _entityId;

        public string RunId => _runId;

        public void SetRunId(string value)
        {
            _runId = value;
        }
    }
}

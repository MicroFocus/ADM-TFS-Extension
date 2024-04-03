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

using PSModule.AlmLabMgmtClient.SDK;
using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Request;
using PSModule.AlmLabMgmtClient.SDK.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.Result
{
    public class LabPublisher : Publisher
    {
        private const string NAME = "name";
        public LabPublisher(IClient client, string entityId, string runId, string nameSuffix) : base(client, entityId, runId, nameSuffix)
        {
        }

        protected async override Task<string> GetEntityName()
        {
            string name = "Unnamed Entity";
            try
            {
                Response response = await GetRunEntityName();
                if (response.IsOK && !response.Data.IsNullOrWhiteSpace())
                {
                    name = Xml.GetAttributeValue(response.Data, NAME);
                }
                else
                {
                    await _logger.LogError($"Failed to get Entity name. Exception: {response.Error}");
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                await _logger.LogError(e.Message);
            }

            return name;
        }

        protected override GetRequest GetRunEntityTestSetRunsRequest(IClient client, string runId)
        {
            return new GetLabRunEntityTestSetRunsRequest(_client, _runId);
        }
    }
}
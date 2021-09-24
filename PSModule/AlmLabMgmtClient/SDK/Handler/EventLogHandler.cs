using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Request;
using PSModule.AlmLabMgmtClient.SDK.Util;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Handler
{
    public class EventLogHandler : HandlerBase
    {
        private const string CREATION_TIME = "creation-time";
        private const string DESCRIPTION = "description";
        private const string ID = "id";

        private int _lastRead = -1;
        private new readonly string _timeslotId;

        public EventLogHandler(IClient client, string timeslotId) : base(client, timeslotId)
        {
            _timeslotId = timeslotId;
        }

        public async Task<bool> Log(bool logRequestUrl)
        {
            bool ok = false;
            Response eventLog = null;
            try
            {
                eventLog = await GetEventLog(logRequestUrl);
                string xml = eventLog.ToString();
                var entities = Xml.ToEntities(xml);
                foreach (var currEntity in entities)
                {
                    if (IsNew(currEntity))
                    {
                        await _logger.LogInfo($"{currEntity[CREATION_TIME]}:{currEntity[DESCRIPTION]}");
                    }
                }
                ok = true;
            }
            catch(ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Failed to print Event Log: {eventLog} (run id: {_runId}, reservation id: {_timeslotId}). Cause: {ex.Message}");

            }

            return ok;
        }

        private bool IsNew(IDictionary<string, string> currEntity)
        {
            bool isNew = false;
            if (currEntity?.ContainsKey(ID) != true)
                throw new AlmException("Current entity is null or does not contain the [id] key", ErrorCategory.InvalidData);
            if (!int.TryParse(currEntity[ID], out int currEvent))
                throw new AlmException($"Current entity has an invalid [id]: {currEntity[ID]}", ErrorCategory.InvalidData);

            if (currEvent > _lastRead)
            {
                _lastRead = currEvent;
                isNew = true;
            }

            return isNew;
        }

        private async Task<Response> GetEventLog(bool logRequestUrl)
        {
            return await new EventLogRequest(_client, _timeslotId).Execute(logRequestUrl);
        }
    }
}
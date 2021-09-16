using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Request;
using PSModule.AlmLabMgmtClient.SDK.Util;
using System;
using System.Collections.Generic;
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

        public async Task<bool> Log()
        {
            bool ok = false;
            Response eventLog = null;
            try
            {
                eventLog = await GetEventLog();
                string xml = eventLog.ToString();
                var entities = Xml.ToEntities(xml);
                foreach (var currEntity in entities)
                {
                    if (IsNew(currEntity))
                    {
                        _logger.LogInfo($"{currEntity[CREATION_TIME]}:{currEntity[DESCRIPTION]}");
                    }
                }
                ok = true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to print Event Log: {eventLog} (run id: {_runId}, reservation id: {_timeslotId}). Cause: {ex.Message}");
            }

            return ok;
        }

        private bool IsNew(IDictionary<string, string> currEntity)
        {
            bool isNew = false;
            int currEvent = int.Parse(currEntity[ID]);
            if (currEvent > _lastRead)
            {
                _lastRead = currEvent;
                isNew = true;
            }

            return isNew;
        }

        private async Task<Response> GetEventLog()
        {
            return await new EventLogRequest(_client, _timeslotId).Execute();
        }
    }
}
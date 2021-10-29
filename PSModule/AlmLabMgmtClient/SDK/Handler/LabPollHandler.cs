using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Request;
using PSModule.AlmLabMgmtClient.SDK.Util;
using System.Threading;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Handler
{
    public class LabPollHandler : PollHandler
    {
        private const string END_TIME = "end-time";
        private const string START_TIME = "start-time";
        private const string STATE = "state";
        private const string COMPLETED_SUCCESSFULLY = "completed-successfully";
        private const string RESERVATION_ID = "reservation-id";

        private EventLogHandler _eventLogHandler;

        public LabPollHandler(IClient client, string entityId) : base(client, entityId)
        {
        }

        public LabPollHandler(IClient client, string entityId, int interval) : base(client, entityId, interval) { }

        protected override async Task<bool> DoPoll()
        {
            bool ok = false;

            Response res = await GetRunEntityData();
            if (res.IsOK)
            {
                SetTimeslotId(res);
                _eventLogHandler = new EventLogHandler(_client, _timeslotId);
                if (!_timeslotId.IsNullOrWhiteSpace())
                {
                    ok = await base.DoPoll();
                }
            }
            else
            {
                LogPollingError(res);
            }
            return ok;
        }

        protected override async Task<Response> GetResponse(bool logRequestUrl)
        {
            return await new PollAlmLabMgmtRunRequest(_client, _runId).Execute(logRequestUrl);
        }

        protected async override Task LogProgress(bool logRequestUrl)
        {
            await _eventLogHandler.Log(logRequestUrl);
        }

        protected override bool IsFinished(Response response)
        {
            bool ret = false;
            try
            {
                string xml = response.ToString();
                string endTime = Xml.GetAttributeValue(xml, END_TIME);
                if (!endTime.IsNullOrWhiteSpace())
                {
                    string startTime = Xml.GetAttributeValue(xml, START_TIME);
                    string currentRunState = Xml.GetAttributeValue(xml, STATE);
                    _logger.LogInfo($"Timeslot {_timeslotId} is [{currentRunState}].\nRun start time: [{startTime}], Run end time: [{endTime}]");
                    ret = true;
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch
            {
                _logger.LogError($"Failed to parse response: {response}");
                ret = true;
            }

            return ret;
        }

        protected override bool LogRunEntityResults(Response response)
        {
            bool ok = false;
            try
            {
                string xml = response.ToString();
                string state = Xml.GetAttributeValue(xml, STATE);
                string completedSuccessfully = Xml.GetAttributeValue(xml, COMPLETED_SUCCESSFULLY);
                _logger.LogDebug($"Run state of {_runId}: {state}, Completed successfully: {completedSuccessfully}");
                ok = true;
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch
            {
                _logger.LogError($"Failed to parse response: {response}");
            }

            return ok;
        }

        private void SetTimeslotId(Response runEntityResponse)
        {
            _timeslotId = GetTimeslotId(runEntityResponse);
            if (!_timeslotId.IsNullOrWhiteSpace())
                _logger.LogInfo($"Timeslot id: {_timeslotId}");
        }

        private async Task<Response> GetRunEntityData()
        {
            return await new GetLabRunEntityDataRequest(_client, _runId).Execute();
        }

        private string GetTimeslotId(Response response)
        {
            string id = string.Empty;
            try
            {
                string xml = response.ToString();
                id = Xml.GetAttributeValue(xml, RESERVATION_ID);
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch
            {
                _logger.LogError($"Failed to parse response for timeslot ID: {response}");
            }

            return id;
        }

        protected override async Task<Response> GetRunEntityResultsResponse()
        {
            return await new GetLabRunEntityDataRequest(_client, _runId).Execute();
        }
    }
}
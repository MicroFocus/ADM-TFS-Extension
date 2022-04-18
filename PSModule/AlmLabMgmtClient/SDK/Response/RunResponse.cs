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
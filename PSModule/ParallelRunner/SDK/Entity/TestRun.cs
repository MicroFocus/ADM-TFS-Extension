using PSModule.ParallelRunner.SDK.Util;
using Newtonsoft.Json;
using PSModule.UftMobile.SDK.Entity;

namespace PSModule.ParallelRunner.SDK.Entity
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class TestRun
    {
        private const string PASSED = "Passed";
        private const string FAILED = "Failed";

        private const string PASS = "pass";
        private const string FAIL = "fail";
        private const string WARNING = "warning";
        private const string ERROR = "error";

        private EnvType _envType = EnvType.None;

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("test.name")]
        public string TestName { get; set; }

        [JsonProperty("test.path")]
        public string TestPath { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("report")]
        public string RunResultsHtmlRelativePath { get; set; }

        [JsonProperty("environment.web.browser")]
        public string Browser { get; set; }

        [JsonProperty("environment.mobile.device")]
        public Device Device { get; set; }

        [JsonProperty("runtime.tooltip")]
        public string Tooltip { get; set; }

        public EnvType GetEnvType()
        {
            if (_envType == EnvType.None)
            {
                if (Browser.IsNullOrEmpty() && Device != null)
                    _envType = EnvType.Mobile;
                else if (!Browser.IsNullOrEmpty() && Device == null)
                    _envType = EnvType.Web;
            }

            return _envType;
        }

        public string GetDetails()
        {
            if (_envType == EnvType.None)
                GetEnvType();

            return _envType == EnvType.Mobile ? Device.ToHtmlString() : (_envType == EnvType.Web ? @$"Browser: <span style=""font-weight:bold"">{Browser}</span>" : string.Empty);
        }

        public string GetAzureStatus()
        {
            if (RunResultsHtmlRelativePath.IsNullOrEmpty())
                return ERROR;

            if (Status.EqualsIgnoreCase(FAILED))
                return FAIL;
            if (Status.EqualsIgnoreCase(PASSED))
                return PASS;
            if (Status.EqualsIgnoreCase(WARNING))
                return WARNING;

            return ERROR;
        }
    }
}
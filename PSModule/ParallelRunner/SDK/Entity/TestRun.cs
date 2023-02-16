using PSModule.ParallelRunner.SDK.Util;
using Newtonsoft.Json;
using PSModule.UftMobile.SDK.Entity;
using System;
using System.Globalization;

namespace PSModule.ParallelRunner.SDK.Entity
{
    //[JsonConverter(typeof(JsonPathConverter))]
    public class TestRun
    {
        private const string PASSED = "Passed";
        private const string FAILED = "Failed";

        private const string PASS = "pass";
        private const string FAIL = "fail";
        private const string WARNING = "warning";
        private const string ERROR = "error";
        private const string SKIPPED = "skipped";
        private const string yyyyMMdd_HHmmss_zzz = "yyyy-MM-dd HH:mm:ss zzz";

        [JsonProperty("id")]
        private int _id;

        [JsonProperty("name")]
        private string _name;

        [JsonProperty("test.name")]
        private string _oldName;

        [JsonProperty("runStartTime")]
        private string _runStartTime;

        [JsonProperty("timeZone")]
        private string _timeZone;

        [JsonProperty("duration")]
        private int _duration;

        [JsonProperty("status")]
        private string _status;

        [JsonProperty("error")]
        private string _error;

        [JsonProperty("path")]
        private string _path;

        [JsonProperty("report")]
        private string _runResultsHtmlRelativePath;

        [JsonProperty("envType")]
        private EnvType _envType;

        [JsonProperty("browser")]
        private string _browser;

        [JsonConverter(typeof(JsonPathConverter))]
        [JsonProperty("device")]
        private Device _device;

        [JsonProperty("environment.web.browser")]
        private string _oldBrowser;

        [JsonProperty("environment.mobile.device")]
        private Device _oldDevice;

        public int Id => _id;

        public string Name => _name ?? _oldName;

        public string RunStartTime => _runStartTime;

        public int Duration => _duration;

        public string Path => _path;

        public string Status => _status;

        public string RunResultsHtmlRelativePath => _runResultsHtmlRelativePath;

        public string Browser => _browser ?? _oldBrowser;

        public Device Device => _device ?? _oldDevice;

        public string Error => _error;

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
            if (_id > 0 && _runResultsHtmlRelativePath.IsNullOrEmpty())
                return ERROR;

            if (Status.EqualsIgnoreCase(FAILED))
                return FAIL;
            if (Status.EqualsIgnoreCase(PASSED))
                return PASS;
            if (Status.EqualsIgnoreCase(WARNING))
                return WARNING;
            if (Status.EqualsIgnoreCase(SKIPPED))
                return SKIPPED;

            return ERROR;
        }

        public TestRun() { }

        public TestRun(string name, string runStartTime, string timeZone, int duration, string status, string error, string path, EnvType envType, string browser, Device device)
        {
            _name = name;
            _runStartTime = runStartTime;
            _timeZone = timeZone;
            _duration = duration;
            _status = status;
            _error = error;
            _path = path;
            _envType = envType;
            _browser = browser;
            _device = device;
        }
        public TestRun(int id, string oldName, string status, string runResultsHtmlRelativePath, string oldBrowser, Device oldDevice)
        {
            _id = id;
            _oldName = oldName;
            _status = status;
            _runResultsHtmlRelativePath = runResultsHtmlRelativePath;
            _oldBrowser = oldBrowser;
            _oldDevice = oldDevice;
        }
    }
}
using PSModule.ParallelRunner.SDK.Util;
using Newtonsoft.Json;
using PSModule.UftMobile.SDK.Entity;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using PSModule.Common;

namespace PSModule.ParallelRunner.SDK.Entity
{
    using C = PSModule.Common.Constants;
    [JsonConverter(typeof(JsonPathConverter))]
    public class TestRun
    {
        private const string PASSED = "Passed";
        private const string FAILED = "Failed";

        private const string PASS = "pass";
        private const string FAIL = "fail";
        private const string WARNING = "warning";
        private const string ERROR = "error";

        private const string DEVICE_ID = "Device ID";
        private const string TYPE = "type";
        private const string USER = "User";
        private const string MANUF = "Manufacturer";
        private const string MODEL = "Model";
        private const string OS = "OS";
        private const string OS_TYPE = "OsType";
        private const string OSVERSION = "OsVersion";
        private const string OS_VERSION = "OS Version";
        private const string REPORT_NODE = "ReportNode";
        private const string DATA = "Data";
        private const string NAME = "Name";
        private const string DESCRIPTION = "Description";
        private const string EXTENSION = "Extension";
        private const string PARALLEL_RUNNER_ENV_INFO = "ParallelRunnerEnvInfo";
        private const string ID = "ID";
        private const char LT = '<';
        private const char GT = '>';
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

        public bool HasValidReport(string parentPath)
        {
            if (_envType == EnvType.None)
                GetEnvType();
            if (_envType == EnvType.Mobile)
            {
                string fullPathOfRunResultsHtml = Path.GetDirectoryName(Path.Combine(parentPath, RunResultsHtmlRelativePath));
                string fullPathOfRunResultsXml = Path.Combine(fullPathOfRunResultsHtml, C.RUN_RESULTS_XML);
                if (File.Exists(fullPathOfRunResultsXml))
                {
                    try
                    {
                        XDocument doc = XDocument.Load(fullPathOfRunResultsXml);
                        var envInfo = doc.Root.Element(REPORT_NODE)?.Element(DATA)?.Element(EXTENSION)?.Element(PARALLEL_RUNNER_ENV_INFO);
                        if (envInfo == null)
                        {
                            var nodes = doc.Root.Descendants(REPORT_NODE).Where(el => el.Attribute(TYPE)?.Value == USER);
                            foreach (var el in nodes)
                            {
                                var data = el.Element(DATA);
                                var name = data?.Element(NAME)?.Value;
                                var val = data?.Element(DESCRIPTION)?.Value.TrimStart(LT).TrimEnd(GT);
                                if (name.EqualsIgnoreCase(DEVICE_ID))
                                {
                                    if (Device.DeviceId.IsNullOrEmpty())
                                        continue;
                                    return Device.DeviceId.EqualsIgnoreCase(val);
                                }
                                else if (name.EqualsIgnoreCase(MANUF))
                                {
                                    return Device.Manufacturer.IsNullOrEmptyOrValue(val);
                                }
                                else if (name.EqualsIgnoreCase(MODEL))
                                {
                                    return Device.Model.IsNullOrEmptyOrValue(val);
                                }
                                else if (name.EqualsIgnoreCase(OS))
                                {
                                    return Device.OSType.IsNullOrEmptyOrValue(val);
                                }
                                else if (name.EqualsIgnoreCase(OS_VERSION))
                                {
                                    IsValidOsVersion(val);
                                }
                            }
                        }
                        else
                        {
                            if (!Device.DeviceId.IsNullOrEmpty())
                            {
                                return envInfo.Element(ID)?.Value.EqualsIgnoreCase(Device.DeviceId) == true;
                            }

                            return ((Device.Manufacturer.IsNullOrEmptyOrValue(envInfo.Element(MANUF)?.Value)) &&
                                    (Device.Model.IsNullOrEmptyOrValue(envInfo.Element(MODEL)?.Value)) &&
                                    (Device.OSType.IsNullOrEmptyOrValue(envInfo.Element(OS_TYPE)?.Value)) &&
                                    IsValidOsVersion(envInfo.Element(OSVERSION)));
                        }
                    }
                    catch
                    {
                        return true;
                    }
                }
            }
            return true; // TODO: WEB env type seems to be OK until now, but will see in future
        }

        private bool IsValidOsVersion(XElement osVerEl)
        {
            if (Device.OSVersion.IsNullOrEmpty() || osVerEl == null || osVerEl.Value.IsNullOrEmpty())
                return true;

            if (double.TryParse(Device.OSVersion, out double v1) && double.TryParse(osVerEl.Value, out double v2))
            {
                return v1 == v2;
            }
            return true; // TODO check deeper since the version can contain math operators
        }
        private bool IsValidOsVersion(string ver)
        {
            if (Device.OSVersion.IsNullOrEmpty() || ver.IsNullOrEmpty())
                return true;

            if (double.TryParse(Device.OSVersion, out double v1) && double.TryParse(ver, out double v2))
            {
                return v1 == v2;
            }
            return true; // TODO check deeper since the version can contain math operators
        }
    }
}
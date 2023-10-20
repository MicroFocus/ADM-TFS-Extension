﻿/*
 * Certain versions of software accessible here may contain branding from Hewlett-Packard Company (now HP Inc.) and Hewlett Packard Enterprise Company.
 * This software was acquired by Micro Focus on September 1, 2017, and is now offered by OpenText.
 * Any reference to the HP and Hewlett Packard Enterprise/HPE marks is historical in nature, and the HP and Hewlett Packard Enterprise/HPE marks are the property of their respective owners.
 * __________________________________________________________________
 * MIT License
 *
 * Copyright 2016-2023 Open Text
 *
 * The only warranties for products and services of Open Text and
 * its affiliates and licensors ("Open Text") are as may be set forth
 * in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or
 * omissions contained herein. The information contained herein is subject
 * to change without notice.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * ___________________________________________________________________
 */

using PSModule.ParallelRunner.SDK.Util;
using Newtonsoft.Json;
using PSModule.UftMobile.SDK.Entity;
using System.IO;

namespace PSModule.ParallelRunner.SDK.Entity
{
    using C = Common.Constants;
    [JsonConverter(typeof(JsonFieldsConverter))]
    public class TestRun
    {
        private const string PASSED = "Passed";
        private const string FAILED = "Failed";

        private const string PASS = "pass";
        private const string FAIL = "fail";
        private const string WARNING = "warning";
        private const string ERROR = "error";
        private const string SKIPPED = "skipped";

        [JsonProperty("id")]
        private int _id;

        [JsonProperty("name")]
        private string _name;

        [JsonProperty("test.name")]
        private string _oldTestName;

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
        private string _oldRunResultsHtmlRelativePath;

        [JsonProperty("envType")]
        private EnvType _envType;

        [JsonProperty("browser")]
        private string _browser;

        [JsonProperty("device")]
        private Device _device;

        [JsonProperty("environment.web.browser")]
        private string _oldBrowser;

        [JsonProperty("environment.mobile.device")]
        private Device _oldDevice;

        #region Properties
        public int Id => _id;

        public string Name => _name ?? _oldTestName;

        public string RunStartTime => _runStartTime;

        public int Duration => _duration;

        public string Path => _path;

        public string Status => _status;

        public string RunResultsHtmlRelativePath => _oldRunResultsHtmlRelativePath;

        public string Browser => _browser ?? _oldBrowser;

        public Device Device => _device ?? _oldDevice;

        public string Error => _error;

        #endregion

        #region Methods

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
            if (_id > 0 && _oldRunResultsHtmlRelativePath.IsNullOrEmpty())
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

        /// <summary>
        /// Check if test run has the html matching report, generated by ParallelRunner
        /// </summary>
        /// <param name="resPath">required for old logic only (based on parallelrun_results.html), example value "D:\Work\UFTTests\MobileConfigP2\ParallelReport\Res5"</param>
        /// the new logic is based on parallelrun_results.json and does not require this param
        /// <returns></returns>
        public bool HasUFTReport(string resPath)
        {
            string fullPathOfRunResults;
            if (_id > 0)
            {
                if (_oldRunResultsHtmlRelativePath.IsNullOrEmpty())
                    return false;
                fullPathOfRunResults = System.IO.Path.Combine(resPath, _oldRunResultsHtmlRelativePath);
            }
            else if (!_path.IsNullOrWhiteSpace())
            {
                fullPathOfRunResults = System.IO.Path.Combine(_path, $@"Report\{C.RUN_RESULTS_HTML}");
            }
            else
            {
                return false;
            }
            return File.Exists(fullPathOfRunResults);
        }

        #endregion

        #region Constructors

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
            _oldTestName = oldName;
            _status = status;
            _oldRunResultsHtmlRelativePath = runResultsHtmlRelativePath;
            _oldBrowser = oldBrowser;
            _oldDevice = oldDevice;
        }

        #endregion
    }
}
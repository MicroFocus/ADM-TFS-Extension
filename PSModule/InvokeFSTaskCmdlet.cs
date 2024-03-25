/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2023 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

using PSModule.Common;
using PSModule.UftMobile.SDK;
using PSModule.UftMobile.SDK.Auth;
using PSModule.UftMobile.SDK.Client;
using PSModule.UftMobile.SDK.Entity;
using PSModule.UftMobile.SDK.Enums;
using PSModule.UftMobile.SDK.Interface;
using PSModule.UftMobile.SDK.UI;
using PSModule.UftMobile.SDK.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Threading.Tasks;
using C = PSModule.Common.Constants;
using Job = PSModule.UftMobile.SDK.Entity.Job;

namespace PSModule
{
    [Cmdlet(VerbsLifecycle.Invoke, "FSTask")]
    public class InvokeFSTaskCmdlet : AbstractLauncherTaskCmdlet
    {
        private const string DEBUG_PREFERENCE = "DebugPreference";
        private const string LOGIN_FAILED = "Login failed";
        private const string MISSING_OR_INVALID_DEVICES = "Missing or invalid devices";
        private const string MISSING_OR_INVALID_APP = "Missing or invalid application";
        private const string MISSING_OR_INVALID_APPS = "Missing or invalid applications";
        private const string MISSING_OR_INVALID_DEVICE = "Missing or invalid device";
        private const string FAILED_TO_CREATE_TEMP_JOB = "Digital Lab server failed to create a temporary job";
        private const string NO_JOB_FOUND_BY_GIVEN_ID = "No job found by given ID";
        private const string APPS_ENDPOINT = "rest/apps/getAplicationsLastVersion";
        private const string APPS_QUERY_PARAMS = "excludeIosAgents=false&multiWorkspace=true";
        private const string GET_APP_BY_ID_ENDPOINT = "rest/apps/getAppById";
        private const string DEVICES_ENDPOINT = "rest/devices";
        private const string DEVICE_ENDPOINT = "rest/device";
        private const string GET_JOB_ENDPOINT = "rest/job";
        private const string CREATE_TEMP_JOB_ENDPOINT = "rest/job/createTempJob";
        //private const string JOB_UPDATE_DEVICES_ENDPOINT = "rest/job/updateDevices";
        private const string JOB_UPDATE_ENDPOINT = "rest/job/updateJob";
        private const string REGISTERED = "registered";
        private const string UNREGISTERED = "unregistered";
        private const string HEAD = "HEAD";
        private const string MISSING_OR_INVALID_CREDENTIALS = "Missing or Invalid Credentials";
        private const string GOOGLE = "https://www.google.com";
        private const string HTTP_PREFIX = "http://";
        private const string HTTPS_PREFIX = "https://";
        private const string JOB_ID = "job_id";
        private const string UPDATE_JOB_DEVICE_FORMAT = @"{{""id"":""{0}"",""capableDeviceFilterDetails"":{{}},""devices"":[{{""deviceID"":""{1}""}}],""application"":{2},""extraApps"":{3},""header"":""{4}""}}";
        private const string UPDATE_JOB_CDFD_FORMAT = @"{{""id"":""{0}"",""capableDeviceFilterDetails"":{1},""application"":{2},""devices"":[],""extraApps"":{3},""header"":""{4}""}}";
        private const string MC_HOME = "MC.Home";
        private IClient _client;
        private IAuthenticator _auth;
        private bool _isLoggedIn;
        private EnvVarsConfig _envVarsConfig;

        [Parameter(Position = 0, Mandatory = true)]
        public string TestsPath { get; set; }

        [Parameter(Position = 1)]
        public string Timeout { get; set; }

        [Parameter(Position = 2, Mandatory = true)]
        public string UploadArtifact { get; set; }

        [Parameter(Position = 3)]
        public ArtifactType ArtType { get; set; }

        [Parameter(Position = 4)]
        public string ReportFileName { get; set; }

        [Parameter(Position = 5)]
        public string ArchiveName { get; set; }

        [Parameter(Position = 6)]
        public string BuildNumber { get; set; }

        [Parameter(Position = 7)]
        public bool EnableFailedTestsReport
        {
            set
            {
                _enableFailedTestsReport = value;
            }
        }

        [Parameter(Position = 8)]
        public bool UseParallelRunner
        {
            set
            {
                _isParallelRunnerMode = value;
            }
        }

        [Parameter(Position = 9)]
        public List<IConfig> Configs
        {
            set
            {
                foreach (IConfig config in value)
                {
                    if (config is ParallelRunnerConfig prConfig)
                    {
                        _parallelRunnerConfig = prConfig;
                    }
                    else if (config is ServerConfigEx srvConfigEx)
                    {
                        _dlServerConfig = srvConfigEx;
                    }
                    else if (config is DeviceConfig dConfig)
                    {
                        _deviceConfig = dConfig;
                    }
                    else if (config is CloudBrowserConfig cbConfig)
                    {
                        _cloudBrowserConfig = cbConfig;
                    }
                    else if (config is EnvVarsConfig evConfig)
                    {
                        _envVarsConfig = evConfig;
                    }
                }
            }
        }

        [Parameter(Position = 10)]
        public List<string> ReportPaths
        {
            set
            {
                _rptPaths = value;
            }
        }

        [Parameter(Position = 11)]
        public bool CancelRunOnFailure { get; set; }

        [Parameter(Position = 12)]
        public string TimestampPattern
        {
            set
            {
                _timestampPattern = value?.Trim();
            }
        }

        protected override bool CollateResults(string resultFile, string resdir)
        {
            return true; //do nothing here. Collate results should be made by the standard "Copy and Publish Artifacts" TFS task
        }

        public override Dictionary<string, string> GetTaskProperties()
        {
            LauncherParamsBuilder builder = new();

            builder.SetRunType(RunType.FileSystem);
            builder.SetPerScenarioTimeOut(Timeout);

            var tests = TestsPath.Split("\n".ToArray());

            for (int i = 0; i < tests.Length; i++)
            {
                string pathToTest = tests[i].Replace("\\", "\\\\");
                builder.SetTest(i + 1, pathToTest);
            }

            builder.SetUploadArtifact(UploadArtifact);
            builder.SetArtifactType(ArtType);
            builder.SetReportName(ReportFileName);
            builder.SetArchiveName(ArchiveName);
            builder.SetEnvVars(_envVarsConfig);
            builder.SetBuildNumber(BuildNumber);
            builder.SetCancelRunOnFailure(CancelRunOnFailure);
            builder.SetEnableFailedTestsReport(_enableFailedTestsReport);
            builder.SetUseParallelRunner(_isParallelRunnerMode);
            if (_isParallelRunnerMode)
            {
                builder.SetParallelRunnerEnvType(_parallelRunnerConfig.EnvType);
                if (_parallelRunnerConfig.EnvType == EnvType.Mobile)
                {
                    var devices = _parallelRunnerConfig.Devices;
                    if (devices.Any())
                    {
                        for (int i = 0; i < tests.Length; i++)
                        {
                            foreach (var (d, j) in devices.WithIndex())
                            {
                                builder.SetParallelTestEnv(i + 1, j + 1, d.ToRawString());
                            }
                        }
                    }
                }
                else if (_parallelRunnerConfig.EnvType == EnvType.Web)
                {
                    var browsers = _parallelRunnerConfig.Browsers;
                    if (browsers.Any())
                    {
                        for (int i = 0; i < tests.Length; i++)
                        {
                            foreach (var (b, j) in browsers.WithIndex())
                            {
                                builder.SetParallelTestEnv(i + 1, j + 1, $"browser: {b}");
                            }
                        }
                    }
                }
            }
            builder.SetDigitalLabSrvConfig(_dlServerConfig);
            builder.SetMobileConfig(_deviceConfig);
            builder.SetCloudBrowserConfig(_cloudBrowserConfig);

            return builder.GetProperties();
        }

        protected override string GetRetCodeFileName()
        {
            return "TestRunReturnCode.txt";
        }

        protected override void ProcessRecord()
        {
            if (_dlServerConfig != null)
            {
                InitRestClientAndLogin().Wait();
                //if DL server type is ValueEdge then GET project fails
                /*if (!IsValidTenantId().Result)
                {
                    ThrowTerminatingError($"{NO_ACTIVE_TENANT_FOUND_BY_GIVEN_ID}: {_dlServerConfig.TenantId}", nameof(IsValidTenantId), ErrorCategory.InvalidData, nameof(IsValidTenantId));
                }*/

                if (_dlServerConfig.UseProxy)
                {
                    try
                    {
                        CheckProxy(_dlServerConfig.ProxyConfig).Wait();
                    }
                    catch (WebException wex)
                    {
                        ThrowTerminatingError(GetErrorFromWebException(wex), nameof(CheckProxy), ErrorCategory.AuthenticationError, nameof(CheckProxy));
                    }
                    catch (Exception ex)
                    {
                        ex = ex.InnerException ?? ex;
                        string err = ex is WebException wex ? GetErrorFromWebException(wex) : $"Proxy Error: {ex.Message}";
                        WriteDebug($"{ex.GetType().Name}: {ex.Message}");
                        ThrowTerminatingError(err, nameof(CheckProxy), ErrorCategory.AuthenticationError, nameof(CheckProxy));
                    }
                }
                if (_isParallelRunnerMode && _parallelRunnerConfig.EnvType == EnvType.Mobile && _parallelRunnerConfig.Devices.Any())
                {
                    List<string> warns = ValidateDeviceLines().Result;
                    if (warns.Any())
                    {
                        warns.ForEach(w => WriteWarning(w));
                    }
                    if (_parallelRunnerConfig.Devices.IsNullOrEmpty())
                    {
                        ThrowTerminatingError(MISSING_OR_INVALID_DEVICES, nameof(ValidateDeviceLines), ErrorCategory.InvalidData, nameof(ValidateDeviceLines));
                    }
                }
                else if (!_isParallelRunnerMode)
                {
                    try
                    {
                        if (_deviceConfig != null)
                        {
                            var device = ValidateDeviceLine().Result;
                            ValidateAndSetApps();
                            var app = _deviceConfig.App;
                            var extraApps = _deviceConfig.ExtraApps;
                            string hdr = GetHeaderJson();
                            Job job = GetOrCreateTempJob().Result;
                            List<Device> jobDevices = job.Devices ?? [];
                            if (device != null) // it means that the deviceId was provided
                            {
                                if (jobDevices.Count != 1 || !jobDevices[0].DeviceId.EqualsIgnoreCase(device.DeviceId))
                                {
                                    UpdateJobDevice(job.Id, device.DeviceId, hdr).Wait();
                                }
                                _deviceConfig.MobileInfo = $"{new MobileInfo(job.Id, device, app: app, extraApps: extraApps, hdr: hdr)}";
                            }
                            else // deviceId was not provided, but other device properties
                            {
                                var cdfDetails = (CapableDeviceFilterDetails)_deviceConfig.Device;
                                UpdateJobCDFDetails(job.Id, cdfDetails, hdr).Wait();
                                _deviceConfig.MobileInfo = $"{new MobileInfo(job.Id, cdfDetails: cdfDetails, app: app, extraApps: extraApps, hdr: hdr)}";
                            }
                        }
                        else if (_cloudBrowserConfig != null)
                        {
                            ValidateCloudBrowser();
                        }
                    }
                    catch (Exception ex)
                    {
                        ex = ex.InnerException ?? ex;
                        string err = ex is WebException wex ? GetErrorFromWebException(wex) : $"Error: {ex.Message}";
                        WriteDebug($"{ex.GetType().Name}: {ex.Message}");
                        WriteDebug(ex.StackTrace);
                        ThrowTerminatingError(err, nameof(ProcessRecord), ErrorCategory.NotSpecified, nameof(ProcessRecord));
                    }
                }
            }
            base.ProcessRecord();
        }

        private string GetErrorFromWebException(WebException wex)
        {
            string err;
            if (wex.Status == WebExceptionStatus.ProtocolError)
            {
                if (wex.Response is HttpWebResponse res)
                {
                    err = res.StatusCode switch
                    {
                        HttpStatusCode.ProxyAuthenticationRequired => MISSING_OR_INVALID_CREDENTIALS,
                        _ => wex.Message,
                    };
                    err = $"Proxy Error: {err}";
                    WriteDebug($"{res.StatusCode}: {wex.Message}");
                }
                else
                {
                    err = $"Proxy Error: {wex.Message}";
                    WriteDebug($"{wex.Status}: {wex.Message}");
                }
            }
            else
            {
                err = $"Proxy Error: {wex.Message}";
                WriteDebug($"{wex.Status}: {wex.Message}");
            }
            return err;
        }

        private async Task CheckProxy(ProxyConfig config)
        {
            await CheckProxy(config.ServerUrl, config.UseCredentials, config.UsernameOrClientId, config.PasswordOrSecret);
        }
        private async Task CheckProxy(string server, bool useCredentials, string username, string password)
        {
            if (server.StartsWith(HTTP_PREFIX) || server.StartsWith(HTTPS_PREFIX))
            {
                throw new ArgumentException(@$"Invalid server name format ""{server}"". The prefix ""http(s)://"" is not expected here.");
            }
            string[] tokens = server.Split(C.COLON);
            if (tokens.Length == 1)
            {
                throw new ArgumentException("Port number is missing. The expected format is [server name or IP]:[port]");
            }
            else if (tokens.Length > 2)
            {
                throw new ArgumentException("Invalid server name format. The expected format is [server name or IP]:[port]");
            }
            else if (!int.TryParse(tokens[1], out int _))
            {
                throw new ArgumentException($"Invalid port value [{tokens[1]}]. A numeric value is expected.");
            }
            var proxy = new WebProxy
            {
                Address = new Uri($"{HTTP_PREFIX}{server}"),
                BypassProxyOnLocal = false
            };
            if (useCredentials)
            {
                proxy.UseDefaultCredentials = false;
                proxy.Credentials = new NetworkCredential(userName: username, password: password);
            }

            using ExWebClient client = new() { Proxy = proxy, Method = HEAD };
            await client.DownloadStringTaskAsync(GOOGLE);
        }

        private void ValidateCloudBrowser()
        {
            WriteDebug("Validating the cloud browser fields ....");
            CloudBrowsers res = GetCloudBrowsers().Result;

            if (res == null)
                ThrowTerminatingError($"Unexpected error during cloud browser validation", nameof(ValidateCloudBrowser), ErrorCategory.InvalidData, nameof(ValidateCloudBrowser));
            else
            {
                if (!res.Browsers.Any(b => b.Type.EqualsIgnoreCase(_cloudBrowserConfig.Browser) && b.IsValidVersionOrTag(_cloudBrowserConfig.Version)))
                {
                    ThrowTerminatingError($"Invalid Cloud Browser: \"{_cloudBrowserConfig.Browser}\" with Version/Tag: \"{_cloudBrowserConfig.Version}\"", nameof(ValidateCloudBrowser), ErrorCategory.InvalidData, nameof(ValidateCloudBrowser));
                }
                if (!_cloudBrowserConfig.OS.In(res.OS, true))
                {
                    ThrowTerminatingError($"Invalid Cloud Browser OS: \"{_cloudBrowserConfig.OS}\"", nameof(ValidateCloudBrowser), ErrorCategory.InvalidData, nameof(ValidateCloudBrowser));
                }
                if (!_cloudBrowserConfig.Region.In(res.Regions, true))
                {
                    ThrowTerminatingError($"Invalid Cloud Browser Region: \"{_cloudBrowserConfig.Region}\"", nameof(ValidateCloudBrowser), ErrorCategory.InvalidData, nameof(ValidateCloudBrowser));
                }
            }
        }

        private async Task<List<string>> ValidateDeviceLines()
        {
            WriteDebug("Validating the devices....");
            List<string> warnings = [];
            GetGroupedDevices(out IList<Device> idDevices, out IList<Device> noIdDevices);
            var devicesWithIdAndOtherProps = idDevices.Where(d => d.HasSecondaryProperties());
            if (devicesWithIdAndOtherProps.Any())
            {
                WriteObject($"DeviceID and other attributes were provided for {devicesWithIdAndOtherProps.Count()} device(s). In this case only the DeviceID will be considered ({devicesWithIdAndOtherProps.Select(d => d.DeviceId).Aggregate((id1, id2) => $"{id1}, {id2}")})");
            }

            var allDevices = await GetAllDevices();
            GetAllDeviceIds(allDevices, out IList<Device> onlineDevices, out IList<Device> offlineDevices);
            if (idDevices.Any())
            {
                var deviceIds = idDevices.Select(d => d.DeviceId).ToList();
                if (offlineDevices.Any())
                {
                    var invalidDeviceIds = deviceIds.Intersect(offlineDevices.Select(d => d.DeviceId));
                    if (invalidDeviceIds.Any())
                    {
                        foreach (var id in invalidDeviceIds)
                        {
                            _parallelRunnerConfig.Devices.RemoveAll(d => d.DeviceId == id);
                            deviceIds.Remove(id);
                            warnings.Add($@"The device with ID ""{id}"" is disconnected, therefore no test run will start for this device");
                        }
                    }
                }
                if (onlineDevices.Any())
                {
                    var invalidDeviceIds = deviceIds.Except(onlineDevices.Select(d => d.DeviceId));
                    if (invalidDeviceIds.Any())
                    {
                        foreach (var id in invalidDeviceIds)
                        {
                            _parallelRunnerConfig.Devices.RemoveAll(d => d.DeviceId == id);
                            warnings.Add(@$"No available device found by ID ""{id}"", therefore no test run will start for this device");
                        }
                    }
                }
                else
                {
                    ThrowTerminatingError($"No available devices found.", nameof(ValidateDeviceLines), ErrorCategory.DeviceError, nameof(ValidateDeviceLines));
                }
            }
            if (noIdDevices.Any())
            {
                foreach (var device in noIdDevices)
                {
                    if (!device.IsAvailable(onlineDevices.AsQueryable(), out string msg))
                    {
                        _parallelRunnerConfig.Devices.Remove(device);
                        warnings.Add($"No available device matches the criteria -> {msg}, therefore no test run will start for this device");
                    }
                }
            }
            return warnings;
        }

        private async Task<Device> ValidateDeviceLine()
        {
            WriteDebug("Validating the device ....");
            Device device = null;
            Device dev = _deviceConfig.Device;
            string err = null;
            if (dev == null)
            {
                err = MISSING_OR_INVALID_DEVICE;
            }
            if (!dev.DeviceId.IsNullOrWhiteSpace() && dev.HasSecondaryProperties())
            {
                WriteObject($"DeviceID and other attributes were provided for device. In this case only the DeviceID will be considered ({dev.DeviceId})");
            }

            if (dev.DeviceId.IsNullOrWhiteSpace())
            {
                var allDevices = await GetAllDevices();
                GetAllDeviceIds(allDevices, out IList<Device> onlineDevices, out IList<Device> offlineDevices);
                if (!dev.IsAvailable(onlineDevices.AsQueryable(), out string msg))
                {
                    err = $"No available device matches the criteria -> {msg}";
                }
            }
            else
            {
                device = await GetDevice(dev.DeviceId);
                if (device == null)
                    err = @$"Device ""{dev.DeviceId}"" not found.";
                else if (device.DeviceStatus == UNREGISTERED)
                    err = $@"The device with ID ""{dev.DeviceId}"" is disconnected";
            }
            if (!err.IsNullOrEmpty())
            {
                ThrowTerminatingError(err, nameof(ValidateDeviceLine), ErrorCategory.InvalidData, nameof(ValidateDeviceLine));
            }
            return device;
        }

        private void GetAllDeviceIds(IList<Device> allDevices, out IList<Device> onlineDevices, out IList<Device> offlineDevices)
        {
            var devices = allDevices.GroupBy(d => d.DeviceStatus)?.ToList() ?? new();
            onlineDevices = devices.FirstOrDefault(g => g.Key == REGISTERED)?.ToList() ?? new();
            offlineDevices = devices.FirstOrDefault(g => g.Key == UNREGISTERED)?.ToList() ?? new();
        }

        private void GetGroupedDevices(out IList<Device> idDevices, out IList<Device> noIdDevices)
        {
            var devices = _parallelRunnerConfig.Devices.GroupBy(d => d.DeviceId.IsNullOrWhiteSpace())?.ToList() ?? new();
            noIdDevices = devices.FirstOrDefault(g => g.Key)?.ToList() ?? new();
            idDevices = devices.FirstOrDefault(g => !g.Key)?.ToList() ?? new();
        }

        private async Task InitRestClientAndLogin()
        {
            bool isDebug = (ActionPreference)GetVariableValue(DEBUG_PREFERENCE) != ActionPreference.SilentlyContinue;
            Credentials cred = new(_dlServerConfig.UsernameOrClientId, _dlServerConfig.PasswordOrSecret, _dlServerConfig.TenantId);
            _client = new RestClient(_dlServerConfig.ServerUrl, cred, new ConsoleLogger(isDebug), _dlServerConfig.AuthType);
            _auth = _dlServerConfig.AuthType == AuthType.Basic ? new BasicAuthenticator() : new OAuth2Authenticator();
            bool ok = await _auth.Login(_client);
            if (ok)
            {
                _isLoggedIn = true;
            }
            else
            {
                ThrowTerminatingError(LOGIN_FAILED, nameof(ValidateDeviceLines), ErrorCategory.DeviceError, nameof(ValidateDeviceLines));
            }
        }

        private void ThrowTerminatingError(string err, string errorId, ErrorCategory errorCategory, string targetObject)
        {
            if (_auth != null && _isLoggedIn)
            {
                _auth.Logout(_client).Wait();
            }
            ThrowTerminatingError(new(new(err), errorId, errorCategory, targetObject));
        }

        private async Task<IList<Device>> GetAllDevices()
        {
            var res = await _client.HttpGet<Device>(DEVICES_ENDPOINT);
            if (res.IsOK)
            {
                return res.Entities;
            }
            else
                ThrowTerminatingError(res.Error, nameof(ValidateDeviceLines), ErrorCategory.DeviceError, nameof(ValidateDeviceLines));

            return [];
        }

        private async Task<Device> GetDevice(string id)
        {
            var res = await _client.HttpGet<Device>($"{DEVICE_ENDPOINT}/{id}", resType: ResType.DataEntity);
            if (res.IsOK)
                return res.Entity;
            else
                ThrowTerminatingError(res.Error, nameof(ValidateDeviceLine), ErrorCategory.DeviceError, nameof(ValidateDeviceLine));

            return null;
        }

        private async Task<App> GetApp(string id)
        {
            var res = await _client.HttpGet<App>($"{GET_APP_BY_ID_ENDPOINT}/{id}", resType: ResType.DataEntity);
            if (res.IsOK)
                return res.Entity;
            else
                ThrowTerminatingError(res.Error, nameof(GetApp), ErrorCategory.NotSpecified, nameof(GetApp));

            return null;
        }

        private async Task<IEnumerable<App>> GetAllApps(bool includeSysApps = false)
        {
            var res = await _client.HttpGet<App>($"{APPS_ENDPOINT}", query: APPS_QUERY_PARAMS);
            if (res.IsOK)
                return includeSysApps ? res.Entities : res.Entities.Where(app => app.Source == C.MC);
            else
                ThrowTerminatingError(res.Error, nameof(GetAllApps), ErrorCategory.NotSpecified, nameof(GetAllApps));

            return null;
        }

        private async Task<Job> CreateTempJob()
        {
            Job job = null;
            var res = await _client.HttpGet<Job>(CREATE_TEMP_JOB_ENDPOINT, resType: ResType.DataEntity);
            if (res.IsOK)
            {
                if (res.Entity != null)
                    job = res.Entity;
                else
                    ThrowTerminatingError(FAILED_TO_CREATE_TEMP_JOB, nameof(CreateTempJob), ErrorCategory.DeviceError, nameof(CreateTempJob));
            }
            else
            {
                ThrowTerminatingError(res.Error, nameof(CreateTempJob), ErrorCategory.DeviceError, nameof(CreateTempJob));
            }
            return job;
        }

        private async Task<Job> GetJob(string jobId)
        {
            var res = await _client.HttpGet<Job>($"{GET_JOB_ENDPOINT}/{jobId}", resType: ResType.DataEntity);
            if (res.IsOK && res.Entities.Any())
            {
                var job = res.Entities[0];
                return job;
            }
            else
            {
                await _client.Logger.LogDebug(res.IsOK ? $"{NO_JOB_FOUND_BY_GIVEN_ID}: {jobId}" : res.Error);
                return null;
            }
        }

        private async Task<Job> GetOrCreateTempJob()
        {
            Job job = null;
            string workDir = _deviceConfig.WorkDir;
            string jobIdFile = Path.Combine(workDir, JOB_ID);
            if (File.Exists(jobIdFile))
            {
                using var sr = File.OpenText(jobIdFile);
                string jobId = sr.ReadLine();
                if (!jobId.IsNullOrWhiteSpace())
                    job = await GetJob(jobId);
            }
            if (job == null)
            {
                job = await CreateTempJob();
                File.WriteAllText(jobIdFile, job.Id);
            }

            return job;
        }

        private async Task UpdateJobDevice(string jobId, string deviceId, string hdr)
        {
            string jsonApp = _deviceConfig.App.Json4JobUpdate;
            string jsonExtraApps = GetExtraAppsJson4JobUpdate();
            var res = await _client.HttpPost(JOB_UPDATE_ENDPOINT, string.Format(UPDATE_JOB_DEVICE_FORMAT, jobId, deviceId, jsonApp, jsonExtraApps, hdr.EscapeDblQuotes()));
            if (!res.IsOK)
            {
                ThrowTerminatingError(res.Error, nameof(UpdateJobDevice), ErrorCategory.NotSpecified, nameof(UpdateJobDevice));
            }
        }

        private async Task UpdateJobCDFDetails(string jobId, CapableDeviceFilterDetails details, string hdr)
        {
            string jsonApp = _deviceConfig.App?.Json4JobUpdate;
            string jsonExtraApps = GetExtraAppsJson4JobUpdate();
            string jsonDetails = details?.ToJson(false);

            string body = string.Format(UPDATE_JOB_CDFD_FORMAT, jobId, jsonDetails, jsonApp, jsonExtraApps, hdr?.EscapeDblQuotes());
            var res = await _client.HttpPost(JOB_UPDATE_ENDPOINT, body);
            if (!res.IsOK)
            {
                ThrowTerminatingError(res.Error, nameof(UpdateJobCDFDetails), ErrorCategory.NotSpecified, nameof(UpdateJobCDFDetails));
            }
        }
        private List<string> ValidateExtraAppLinesAndSetExtraApps(IEnumerable<App> allApps)
        {
            WriteDebug("Validating the extra apps ....");
            List<string> warnings = [];
            var extraAppLines = _deviceConfig.ExtraAppLines;

            foreach (var appLine in extraAppLines)
            {
                if (appLine.IsAvailable(allApps.AsQueryable(), out App app, out string warning))
                {
                    app.Instrumented = appLine.UsePackaged;
                    _deviceConfig.ExtraApps.Add(app);
                }
                else
                {
                    warnings.Add($"No available app matches the criteria -> {warning}, therefore this app will be ignored");
                }
            }
            return warnings;
        }
        private void ValidateAndSetApps()
        {
            App app;
            if (_deviceConfig.AppType != AppType.Custom)
            {
                var appId = _deviceConfig.AppType == AppType.System ? _deviceConfig.SysApp.GetStringValue() : MC_HOME;
                app = GetApp(appId).Result;
                if (app == null)
                {
                    ThrowTerminatingError($"{MISSING_OR_INVALID_APP}: [{appId}]", nameof(ValidateAndSetApps), ErrorCategory.InvalidData, nameof(ValidateAndSetApps));
                }
                _deviceConfig.App = app;
            }
            if (_deviceConfig.AppLine != null || _deviceConfig.ExtraAppLines.Any())
            {
                var allApps = GetAllApps().Result;
                if (_deviceConfig.AppType == AppType.Custom)
                {
                    if (_deviceConfig.AppLine.IsAvailable(allApps.AsQueryable(), out app, out string msg))
                    {
                        app.Instrumented = _deviceConfig.AppLine.UsePackaged;
                        _deviceConfig.App = app;
                    }
                    else
                    {
                        ThrowTerminatingError($"{MISSING_OR_INVALID_APP}: {msg}", nameof(ValidateAndSetApps), ErrorCategory.InvalidData, nameof(ValidateAndSetApps));
                    }
                }
                var warns = ValidateExtraAppLinesAndSetExtraApps(allApps);
                if (warns.Any())
                {
                    warns.ForEach(w => WriteWarning(w));
                }
            }
        }

        private string GetExtraAppsJson4JobUpdate()
        {
            List<string> list = [];
            _deviceConfig.ExtraApps.ForEach(a => list.Add(a.Json4JobUpdate));
            return $"[{string.Join(C.COMMA, list)}]";
        }
        private string GetHeaderJson()
        {
            Header hdr = new() { DeviceMetrics = _deviceConfig.DeviceMetrics, AppAction = _deviceConfig.AppAction };
            return hdr.ToJson(indented: false);
        }

        private async Task<CloudBrowsers> GetCloudBrowsers()
        {
            var res = await _client.HttpGet<CloudBrowsers>(C.BROWSERS_ENDPOINT, query: C.TOOL_VERSION, resType: ResType.Object);
            if (res.IsOK)
            {
                var data = res.Entity;
                if (data?.Browsers?.Length > 0)
                {
                    return data;
                }
                else
                {
                    ThrowTerminatingError(C.NO_BROWSER_FOUND, nameof(ValidateCloudBrowser), ErrorCategory.InvalidData, nameof(ValidateCloudBrowser));
                }
            }
            else
                ThrowTerminatingError(res.Error, nameof(ValidateCloudBrowser), ErrorCategory.DeviceError, nameof(ValidateCloudBrowser));

            return null;
        }
    }
}

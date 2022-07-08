using System.Management.Automation;
using System.Linq;
using System.Collections.Generic;
using PSModule.UftMobile.SDK.UI;
using PSModule.UftMobile.SDK.Interface;
using PSModule.UftMobile.SDK.Auth;
using PSModule.UftMobile.SDK.Util;
using PSModule.UftMobile.SDK;
using System.Threading.Tasks;
using PSModule.UftMobile.SDK.Entity;
using System;
using System.Net;
using PSModule.UftMobile.SDK.Client;
using Job = PSModule.UftMobile.SDK.Entity.Job;
using C = PSModule.Common.Constants;
using System.IO;
using PSModule.UftMobile.SDK.Enums;

namespace PSModule
{
    [Cmdlet(VerbsLifecycle.Invoke, "FSTask")]
    public class InvokeFSTaskCmdlet : AbstractLauncherTaskCmdlet
    {
        private const string DEBUG_PREFERENCE = "DebugPreference";
        private const string LOGIN_FAILED = "Login failed";
        private const string MISSING_OR_INVALID_DEVICES = "Missing or invalid devices";
        private const string MISSING_OR_INVALID_DEVICE = "Missing or invalid device";
        private const string FAILED_TO_CREATE_TEMP_JOB = "UFT Mobile server failed to create a temp job";
        private const string NO_JOB_FOUND_BY_GIVEN_ID = "No job found by given ID";
        private const string DEVICES_ENDPOINT = "rest/devices";
        private const string GET_JOB_ENDPOINT = "rest/job";
        private const string CREATE_TEMP_JOB_ENDPOINT = "rest/job/createTempJob";
        private const string UPDATE_JOB_ENDPOINT = "rest/job/updateJob/";
        private const string REGISTERED = "registered";
        private const string UNREGISTERED = "unregistered";
        private const string HEAD = "HEAD";
        private const string MISSING_OR_INVALID_CREDENTIALS = "Missing or Invalid Credentials";
        private const string GOOGLE = "https://www.google.com";
        private const string HTTP_PREFIX = "http://";
        private const string HTTPS_PREFIX = "https://";
        private const string JOB_ID = "job_id";

        private IClient _client;
        private IAuthenticator _auth;
        private bool _isLoggedIn;

        [Parameter(Position = 0, Mandatory = true)]
        public string TestsPath { get; set; }

        [Parameter(Position = 1)]
        public string Timeout { get; set; }

        [Parameter(Position = 2, Mandatory = true)]
        public string UploadArtifact { get; set; }

        [Parameter(Position = 3)]
        public ArtifactType ArtType { get; set; }

        [Parameter(Position = 4)]
        public string StorageAccount { get; set; }

        [Parameter(Position = 5)]
        public string Container { get; set; }

        [Parameter(Position = 6)]
        public string ReportFileName { get; set; }

        [Parameter(Position = 7)]
        public string ArchiveName { get; set; }

        [Parameter(Position = 8)]
        public string BuildNumber { get; set; }

        [Parameter(Position = 9)]
        public bool EnableFailedTestsReport
        {
            get
            {
                return _enableFailedTestsReport;
            }
            set
            {
                _enableFailedTestsReport = value;
            }
        }

        [Parameter(Position = 10)]
        public bool UseParallelRunner
        {
            get
            {
                return _isParallelRunnerMode;
            }
            set
            {
                _isParallelRunnerMode = value;
            }
        }

        [Parameter(Position = 11)]
        public ParallelRunnerConfig ParallelRunnerConfig { get; set; }

        [Parameter(Position = 12)]
        public List<string> ReportPaths
        {
            get
            {
                return _rptPaths;
            }
            set
            {
                _rptPaths = value;
            }
        }

        [Parameter(Position = 13)]
        public MobileConfig MobileConfig
        {
            get
            {
                return _mobileConfig;
            }
            set
            {
                _mobileConfig = value;
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
            builder.SetStorageAccount(StorageAccount);
            builder.SetContainer(Container);
            builder.SetBuildNumber(BuildNumber);
            builder.SetEnableFailedTestsReport(EnableFailedTestsReport);
            builder.SetUseParallelRunner(_isParallelRunnerMode);
            if (_isParallelRunnerMode)
            {
                builder.SetParallelRunnerEnvType(ParallelRunnerConfig.EnvType);
                if (ParallelRunnerConfig.EnvType == EnvType.Mobile)
                {
                    var devices = ParallelRunnerConfig.Devices;
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
                else if (ParallelRunnerConfig.EnvType == EnvType.Web)
                {
                    var browsers = ParallelRunnerConfig.Browsers;
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
            if (_mobileConfig != null)
            {
                builder.SetMobileConfig(_mobileConfig);
            }

            return builder.GetProperties();
        }

        protected override string GetRetCodeFileName()
        {
            return "TestRunReturnCode.txt";
        }

        protected override void ProcessRecord()
        {
            if (MobileConfig != null)
            {
                InitRestClientAndLogin().Wait();
                if (MobileConfig.UseProxy)
                {
                    try
                    {
                        CheckProxy(MobileConfig.ProxyConfig).Wait();
                    }
                    catch (WebException wex)
                    {
                        ThrowTerminatingError(new(new(GetErrorFromWebException(wex)), nameof(CheckProxy), ErrorCategory.AuthenticationError, nameof(CheckProxy)));
                    }
                    catch (Exception ex)
                    {
                        ex = ex.InnerException ?? ex;
                        string err = ex is WebException wex ? GetErrorFromWebException(wex) : $"Proxy Error: {ex.Message}";
                        WriteDebug($"{ex.GetType().Name}: {ex.Message}");
                        ThrowTerminatingError(new(new(err), nameof(CheckProxy), ErrorCategory.AuthenticationError, nameof(CheckProxy)));
                    }
                }
                if (_isParallelRunnerMode && ParallelRunnerConfig.EnvType == EnvType.Mobile && ParallelRunnerConfig.Devices.Any())
                {
                    List<string> warns = ValidateDevices().Result;
                    if (warns.Any())
                    {
                        warns.ForEach(w => WriteWarning(w));
                    }
                    if (ParallelRunnerConfig.Devices.IsNullOrEmpty())
                    {
                        ThrowTerminatingError(MISSING_OR_INVALID_DEVICES, nameof(ValidateDevices), ErrorCategory.InvalidData, nameof(ValidateDevices));
                    }
                }
                else if (!_isParallelRunnerMode)
                {
                    ValidateDevice().Wait();
                    //TODO
                    //Job job = GetOrCreateTempJob().Result;
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

        private async Task<List<string>> ValidateDevices()
        {
            WriteDebug("Validating the devices....");
            List<string> warnings = new();
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
                            ParallelRunnerConfig.Devices.RemoveAll(d => d.DeviceId == id);
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
                            ParallelRunnerConfig.Devices.RemoveAll(d => d.DeviceId == id);
                            warnings.Add(@$"No available device found by ID ""{id}"", therefore no test run will start for this device");
                        }
                    }
                }
                else
                {
                    ThrowTerminatingError(new(new($"No available devices found."), nameof(ValidateDevices), ErrorCategory.DeviceError, nameof(ValidateDevices)));
                }
            }
            if (noIdDevices.Any())
            {
                foreach (var device in noIdDevices)
                {
                    if (!device.IsAvailable(onlineDevices.AsQueryable(), out string msg))
                    {
                        ParallelRunnerConfig.Devices.Remove(device);
                        warnings.Add($"No available device matches the criteria -> {msg}, therefore no test run will start for this device");
                    }
                }
            }
            return warnings;
        }

        private async Task ValidateDevice()
        {
            WriteDebug("Validating the device ....");
            Device dev = MobileConfig.Device;
            if (dev == null)
            {
                ThrowTerminatingError(new(new(MISSING_OR_INVALID_DEVICE), nameof(ValidateDevices), ErrorCategory.InvalidData, nameof(ValidateDevices)));
            }
            if (!dev.DeviceId.IsNullOrWhiteSpace() && dev.HasSecondaryProperties())
            {
                WriteObject($"DeviceID and other attributes were provided for device. In this case only the DeviceID will be considered ({dev.DeviceId})");
            }

            var allDevices = await GetAllDevices();
            GetAllDeviceIds(allDevices, out IList<Device> onlineDevices, out IList<Device> offlineDevices);
            if (dev.DeviceId.IsNullOrWhiteSpace())
            {
                if (!dev.IsAvailable(onlineDevices.AsQueryable(), out string msg))
                {
                    ThrowTerminatingError(new(new($"No available device matches the criteria -> {msg}"), nameof(ValidateDevices), ErrorCategory.InvalidData, nameof(ValidateDevices)));
                }
            }
            else
            {
                if (offlineDevices.Any() && offlineDevices.Where(d => d.DeviceId == dev.DeviceId).Any())
                {
                    ThrowTerminatingError(new(new($@"The device with ID ""{dev.DeviceId}"" is disconnected"), nameof(ValidateDevices), ErrorCategory.InvalidData, nameof(ValidateDevices)));
                }
                if (onlineDevices.Any())
                {
                    if (!onlineDevices.Where(d => d.DeviceId == dev.DeviceId).Any())
                        ThrowTerminatingError(new(new(@$"No available device found by ID ""{dev.DeviceId}"""), nameof(ValidateDevices), ErrorCategory.InvalidData, nameof(ValidateDevices)));
                }
                else
                {
                    ThrowTerminatingError(new(new($"No available devices found."), nameof(ValidateDevices), ErrorCategory.DeviceError, nameof(ValidateDevices)));
                }
            }
        }

        private void GetAllDeviceIds(IList<Device> allDevices, out IList<Device> onlineDevices, out IList<Device> offlineDevices)
        {
            var devices = allDevices.GroupBy(d => d.DeviceStatus)?.ToList() ?? new();
            onlineDevices = devices.FirstOrDefault(g => g.Key == REGISTERED)?.ToList() ?? new();
            offlineDevices = devices.FirstOrDefault(g => g.Key == UNREGISTERED)?.ToList() ?? new();
        }

        private void GetGroupedDevices(out IList<Device> idDevices, out IList<Device> noIdDevices)
        {
            var devices = ParallelRunnerConfig.Devices.GroupBy(d => d.DeviceId.IsNullOrWhiteSpace())?.ToList() ?? new();
            noIdDevices = devices.FirstOrDefault(g => g.Key)?.ToList() ?? new();
            idDevices = devices.FirstOrDefault(g => !g.Key)?.ToList() ?? new();
        }

        private async Task InitRestClientAndLogin()
        {
            bool isDebug = (ActionPreference)GetVariableValue(DEBUG_PREFERENCE) != ActionPreference.SilentlyContinue;
            Credentials cred = new(_mobileConfig.UsernameOrClientId, _mobileConfig.PasswordOrSecret, _mobileConfig.TenantId);
            _client = new RestClient(_mobileConfig.ServerUrl, cred, new ConsoleLogger(isDebug), _mobileConfig.AuthType);
            _auth = _mobileConfig.AuthType == AuthType.Basic ? new BasicAuthenticator() : new OAuth2Authenticator();
            bool ok = await _auth.Login(_client);
            if (ok)
            {
                _isLoggedIn = true;
            }
            else
            {
                ThrowTerminatingError(new(new(LOGIN_FAILED), nameof(ValidateDevices), ErrorCategory.DeviceError, nameof(ValidateDevices)));
            }
        }

        private void ThrowTerminatingError(string err, string errorId, ErrorCategory errorCategory, string targetObject)
        {
            if (_auth != null && _isLoggedIn)
            {
                _auth.Logout(_client);
            }
            ThrowTerminatingError(err, errorId, errorCategory, targetObject);
        }

        private async Task<IList<Device>> GetAllDevices()
        {
            var res = await _client.HttpGet<Device>(DEVICES_ENDPOINT);
            if (res.IsOK)
            {
                return res.Entities;
            }
            else
                ThrowTerminatingError(res.Error, nameof(ValidateDevices), ErrorCategory.DeviceError, nameof(ValidateDevices));

            return new List<Device>();
        }

        private async Task<Job> CreateTempJob()
        {
            Job job = null;
            var res = await _client.HttpGet<Job>(CREATE_TEMP_JOB_ENDPOINT, resType: ResType.DataEntity);
            if (res.IsOK)
            {
                if (res.Entities.Any() && res.Entities[0] != null)
                    job = res.Entities[0];
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
            WriteDebug($"GetJob: {jobId}");
            var res = await _client.HttpGet<Job>($"{GET_JOB_ENDPOINT}/{jobId}", resType: ResType.DataEntity);
            if (res.IsOK && res.Entities.Any())
            {
                var job = res.Entities[0];
                WriteDebug($"Found job: {job}");
                return job;
            }
            else 
            {
                WriteDebug(res.IsOK ? $"{NO_JOB_FOUND_BY_GIVEN_ID}: {jobId}" : res.Error);
                return null;
            }
        }

        private async Task<Job> GetOrCreateTempJob()
        {
            Job job = null;
            string workDir = MobileConfig.WorkDir;
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

        private async Task<Job> UpdateJob(Job job)
        {
            //TODO
            throw new NotImplementedException();
        }
    }
}

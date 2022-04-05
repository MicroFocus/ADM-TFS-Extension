using System.Management.Automation;
using System.Linq;
using System.Collections.Generic;
using PSModule.UftMobile.SDK.UI;
using System;
using PSModule.UftMobile.SDK.Interface;
using PSModule.UftMobile.SDK.Auth;
using PSModule.UftMobile.SDK.Util;
using PSModule.UftMobile.SDK;
using System.Threading.Tasks;
using PSModule.UftMobile.SDK.Entity;

namespace PSModule
{
    [Cmdlet(VerbsLifecycle.Invoke, "FSTask")]
    public class InvokeFSTaskCmdlet : AbstractLauncherTaskCmdlet
    {
        private const string DEBUG_PREFERENCE = "DebugPreference";
        private const string LOGIN_FAILED = "Login failed";
        private const string DEVICES_ENDPOINT = "rest/devices";
        private const string REGISTERED = "registered";
        private const string UNREGISTERED = "unregistered";

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
        public IList<string> ReportPaths
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
                            for (int j = 0; j < devices.Count; j++)
                            {
                                builder.SetParallelTestEnv(i+1, j+1, devices[j].ToRawString());
                            }
                        }
                    }
                }
                else if (ParallelRunnerConfig.EnvType == EnvType.Web)
                {
                    //TOOO
                }
            }
            builder.SetMobileConfig(_mobileConfig);

            return builder.GetProperties();
        }

        protected override string GetRetCodeFileName()
        {
            return "TestRunReturnCode.txt";
        }

        protected override void ProcessRecord()
        {
            if (_isParallelRunnerMode)
            {
                ValidateDevices().Wait();
            }
            base.ProcessRecord();
        }

        private async Task ValidateDevices()
        {
            WriteDebug("Validating the devices....");
            var deviceIds = ParallelRunnerConfig.Devices.Select(d => d.DeviceId).Where(id => !string.IsNullOrWhiteSpace(id));
            if (deviceIds.Any())
            {
                var allDevices = await GetAllDevices();
                GetAllDeviceIds(allDevices, out IList<string> onlineDeviceIds, out IList<string> offlineDeviceIds);
                if (offlineDeviceIds.Any())
                {
                    var invalidDeviceIds = deviceIds.Intersect(offlineDeviceIds);
                    if (invalidDeviceIds.Any())
                    {
                        var ids = invalidDeviceIds.Aggregate((a, b) => $"{a}, {b}");
                        ThrowTerminatingError(new(new($"Disconnected devices are not allowed: {ids}"), nameof(ValidateDevices), ErrorCategory.InvalidData, nameof(ValidateDevices)));
                    }

                }
                if (onlineDeviceIds.Any())
                {
                    var invalidDeviceIds = deviceIds.Except(onlineDeviceIds);
                    if (invalidDeviceIds.Any())
                    {
                        var ids = invalidDeviceIds.Aggregate((a, b) => $"{a}, {b}");
                        ThrowTerminatingError(new(new($"Invalid device IDs: {ids}"), nameof(ValidateDevices), ErrorCategory.InvalidData, nameof(ValidateDevices)));
                    }
                }
                else
                {
                    ThrowTerminatingError(new(new($"No available devices found."), nameof(ValidateDevices), ErrorCategory.DeviceError, nameof(ValidateDevices)));
                }
            }
        }

        private void GetAllDeviceIds(IList<Device> allDevices, out IList<string> onlineDeviceIds, out IList<string> offlineDeviceIds)
        {
            var devices = allDevices.GroupBy(d => d.DeviceStatus).ToList();
            onlineDeviceIds = devices.FirstOrDefault(g => g.Key == REGISTERED)?.Select(d => d.DeviceId).ToList() ?? new();
            offlineDeviceIds = devices.FirstOrDefault(g => g.Key == UNREGISTERED)?.Select(d => d.DeviceId).ToList() ?? new();
        }

        private async Task<IList<Device>> GetAllDevices()
        {
            IAuthenticator auth = new BasicAuthenticator();
            Credentials cred = new (_mobileConfig.Username, _mobileConfig.Password);
            bool isDebug = (ActionPreference)GetVariableValue(DEBUG_PREFERENCE) != ActionPreference.SilentlyContinue;
            IClient client = new RestClient(_mobileConfig.ServerUrl, cred, new ConsoleLogger(isDebug));
            bool ok = await auth.Login(client);
            if (ok)
            {
                var res = await client.HttpGet<Device>(client.ServerUrl.AppendSuffix(DEVICES_ENDPOINT));
                if (res.IsOK)
                {
                    return res.Entities;
                }
                else
                    ThrowTerminatingError(new(new(res.Error), nameof(ValidateDevices), ErrorCategory.DeviceError, nameof(ValidateDevices)));

                await auth.Logout(client);
            }
            else
                ThrowTerminatingError(new(new(LOGIN_FAILED), nameof(ValidateDevices), ErrorCategory.DeviceError, nameof(ValidateDevices)));

            return new List<Device>();
        }
    }
}

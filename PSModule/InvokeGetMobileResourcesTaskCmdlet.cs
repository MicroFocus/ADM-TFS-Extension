using PSModule.UftMobile.SDK;
using PSModule.UftMobile.SDK.Auth;
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using C = PSModule.Common.Constants;

namespace PSModule
{
    [Cmdlet(VerbsLifecycle.Invoke, "GMRTask")]
    public class InvokeGetMobileResourcesTaskCmdlet : AsyncCmdlet
    {
        private const string DEVICES_ENDPOINT = "rest/devices";
        private const string APPS_ENDPOINT = "rest/apps/getAplicationsLastVersion";
        private const string APPS_QUERY_PARAMS = "excludeIosAgents=false&multiWorkspace=true";
        private const string REGISTERED = "registered";
        private const string UNREGISTERED = "unregistered";
        private const string AVAILABLE = "Available";
        private const string DISCONNECTED = "Disconnected";
        private const string DEBUG_PREFERENCE = "DebugPreference";
        private const string UFT_LAUNCHER = "UFT_LAUNCHER";
        private const string RUN_STATUS_CODE_TXT = "RunStatusCode.txt";
        private const string NO_DEVICE_FOUND = "No device has been retrieved from the Digital Lab server";
        private const string NO_AVAILABLE_DEVICE_FOUND = "No available device has been retrieved from the Digital Lab server";
        private const string NO_DISCONNECTED_DEVICE_FOUND = "No disconnected device has been retrieved from the Digital Lab server";
        private const string NO_APP_FOUND = "No application has been retrieved from the Digital Lab server";
        private const string LOGIN_FAILED = "Login failed";
        private const string DEVICES_HEAD = "================================== Devices ===================================";
        private const string APPS_HEAD = "================================== Applications ==============================";
        private const string RESOURCES_BOTTOM = "==============================================================================";

        private MobileResxConfig _config;

        [Parameter(Position = 0, Mandatory = true)]
        public MobileResxConfig Config {
            get { return _config; }
            set { _config = value; }
        }

        [Parameter(Position = 1, Mandatory = true)]
        public string BuildNumber { get; set; }

        protected override async Task ProcessRecordAsync()
        {
            try
            {
                WriteDebug($"Username / ClientId = {_config.UsernameOrClientId}");
                bool isDebug = (ActionPreference)GetVariableValue(DEBUG_PREFERENCE) != ActionPreference.SilentlyContinue;
                string ufttfsdir = Environment.GetEnvironmentVariable(UFT_LAUNCHER);
                string resdir = Path.GetFullPath(Path.Combine(ufttfsdir, $@"res\Report_{BuildNumber}"));

                if (!Directory.Exists(resdir))
                    Directory.CreateDirectory(resdir);

                RunStatus runStatus = RunStatus.FAILED, runStatusDevices = RunStatus.PASSED, runStatusApps = RunStatus.PASSED;

                IAuthenticator auth = _config.AuthType == AuthType.Basic ? new BasicAuthenticator() : new OAuth2Authenticator();
                Credentials cred = new(_config.UsernameOrClientId, _config.PasswordOrSecret, _config.TenantId);
                IClient client = new RestClient(_config.ServerUrl, cred, new ConsoleLogger(isDebug), _config.AuthType);
                bool ok = await auth.Login(client);
                if (ok)
                {
                    if (_config.Resx == Resx.OnlyDevices || _config.Resx == Resx.BothDevicesAndApps)
                        runStatusDevices = await CheckAndPrintDevices(client);
                    if (_config.Resx == Resx.OnlyApps || _config.Resx == Resx.BothDevicesAndApps)
                        runStatusApps = await GetAndPrintApps(client);
                    await auth.Logout(client);
                }
                else
                    LogError(new UftMobileException(LOGIN_FAILED));

                if (runStatusDevices == RunStatus.PASSED && runStatusApps == RunStatus.PASSED)
                    runStatus = RunStatus.PASSED;
                else if (runStatusDevices == RunStatus.FAILED && runStatusApps == RunStatus.FAILED)
                    runStatus = RunStatus.FAILED;
                else
                    runStatus = RunStatus.UNSTABLE;

                await SaveRunStatus(resdir, runStatus);
            }
            catch (ThreadInterruptedException e)
            {
                LogError(e, ErrorCategory.OperationStopped);
            }
        }

        private async Task<RunStatus> CheckAndPrintDevices(IClient client)
        {
            RunStatus status = RunStatus.FAILED;
            WriteObject(DEVICES_HEAD);
            var res = await client.HttpGet<Device>(DEVICES_ENDPOINT);
            if (res.IsOK)
            {
                if (res.Entities.Any())
                {
                    if (_config.IncludeOfflineDevices)
                    {
                        var devices = res.Entities.GroupBy(d => d.DeviceStatus).ToList();
                        if (!PrintDevices(devices.FirstOrDefault(g => g.Key == REGISTERED)))
                        {
                            WriteObject(NO_AVAILABLE_DEVICE_FOUND);
                        }
                        if (!PrintDevices(devices.FirstOrDefault(g => g.Key == UNREGISTERED), false))
                        {
                            WriteObject(NO_DISCONNECTED_DEVICE_FOUND);
                        }
                    }
                    else if (!PrintDevices(res.Entities.Where(d => d.DeviceStatus == REGISTERED)))
                    {
                        LogError(new UftMobileException(NO_AVAILABLE_DEVICE_FOUND));
                    }
                }
                else
                {
                    LogError(new UftMobileException(_config.IncludeOfflineDevices ? NO_DEVICE_FOUND : NO_AVAILABLE_DEVICE_FOUND));
                }
                status = RunStatus.PASSED;
            }
            else
            {
                LogError(new UftMobileException($"StatusCode={res.StatusCode}, Error={res.Error}"));
            }

            WriteObject(RESOURCES_BOTTOM);
            return status;
        }

        private bool PrintDevices(IEnumerable<Device> devices, bool isOnline = true)
        {
            if (devices?.Any() == true)
            {
                WriteObject($"{(isOnline ? AVAILABLE : DISCONNECTED)} devices ({devices.Count()}):");
                int x = 0;
                devices.ForEach(d => WriteObject($"Device #{++x} - {d}"));
                return true;
            }
            return false;
        }

        private async Task<RunStatus> GetAndPrintApps(IClient client)
        {
            RunStatus status = RunStatus.FAILED;
            WriteObject(APPS_HEAD);
            var res = await client.HttpGet<App>(APPS_ENDPOINT, query: APPS_QUERY_PARAMS);
            if (res.IsOK)
            {
                var apps = res.Entities.Where(app => app.Source == C.MC);
                if (apps.Any())
                {
                    BaseWriteObject($"Available applications ({apps.Count()}):");
                    int x = 0;
                    apps.ForEach(app => BaseWriteObject($"App #{++x} - {app}"));

                    return RunStatus.PASSED;
                }
                else
                {
                    WriteObject(NO_APP_FOUND);
                    status = RunStatus.PASSED;
                }
            }
            else
            {
                LogError(new UftMobileException($"StatusCode={res.StatusCode}, Error={res.Error}"));
            }

            WriteObject(RESOURCES_BOTTOM);
            return status;
        }

        private async Task SaveRunStatus(string resdir, RunStatus runStatus)
        {
            string fileName = Path.Combine(resdir, RUN_STATUS_CODE_TXT);
            if (!Directory.Exists(resdir))
            {
                ThrowTerminatingError(new ErrorRecord(new DirectoryNotFoundException($"The result folder {resdir} cannot be found."), nameof(SaveRunStatus), ErrorCategory.ResourceUnavailable, nameof(SaveRunStatus)));
                return;
            }
            string retCodeFilename = Path.Combine(resdir, fileName);
            try
            {
                using StreamWriter file = new StreamWriter(retCodeFilename, true);
                await file.WriteLineAsync($"{(int)runStatus}");
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                LogError(e, ErrorCategory.WriteError);
            }
        }

        protected void LogError(Exception ex, ErrorCategory categ = ErrorCategory.NotSpecified, [CallerMemberName] string methodName = "")
        {
            WriteError(new ErrorRecord(ex, $"{ex.GetType()}", categ, methodName));
        }
    }
}
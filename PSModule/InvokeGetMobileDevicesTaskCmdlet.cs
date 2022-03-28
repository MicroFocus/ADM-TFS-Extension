using PSModule.UftMobile.SDK;
using PSModule.UftMobile.SDK.Auth;
using PSModule.UftMobile.SDK.Entity;
using PSModule.UftMobile.SDK.Interface;
using PSModule.UftMobile.SDK.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSModule
{
    [Cmdlet(VerbsLifecycle.Invoke, "GMDTask")]
    public class InvokeGetMobileDevicesTaskCmdlet : AsyncCmdlet
    {
        private const string DEVICES_ENDPOINT = "rest/devices";
        private const string REGISTERED = "registered";
        private const string UNREGISTERED = "unregistered";
        private const string AVAILABLE = "Available";
        private const string DISCONNECTED = "Disconnected";
        private const string DEBUG_PREFERENCE = "DebugPreference";
        private const string UFT_LAUNCHER = "UFT_LAUNCHER";
        private const string RUN_STATUS_CODE_TXT = "RunStatusCode.txt";
        private const string NO_DEVICE_FOUND = "No device has been retrieved from the Mobile Center server";
        private const string NO_AVAILABLE_DEVICE_FOUND = "No available device has been retrieved from the Mobile Center server";
        private const string NO_DISCONNECTED_DEVICE_FOUND = "No disconnected device has been retrieved from the Mobile Center server";
        private const string LOGIN_FAILED = "Login failed";

        [Parameter(Position = 0, Mandatory = true)]
        public string ServerlUrl { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        public string Username { get; set; }

        [Parameter(Position = 2, Mandatory = true)]
        public string Password { get; set; }

        [Parameter(Position = 3, Mandatory = true)]
        public bool IncludeOfflineDevices { get; set; }

        [Parameter(Position = 4, Mandatory = true)]
        public string BuildNumber { get; set; }

        protected override async Task ProcessRecordAsync()
        {
            try
            {
                WriteDebug($"Username = {Username}");
                string ufttfsdir = Environment.GetEnvironmentVariable(UFT_LAUNCHER);
                string resdir = Path.GetFullPath(Path.Combine(ufttfsdir, $@"res\Report_{BuildNumber}"));

                if (!Directory.Exists(resdir))
                    Directory.CreateDirectory(resdir);

                RunStatus runStatus = RunStatus.FAILED;

                IAuthenticator auth = new BasicAuthenticator();
                Credentials cred = new Credentials(Username, Password);
                bool isDebug = (ActionPreference)GetVariableValue(DEBUG_PREFERENCE) != ActionPreference.SilentlyContinue;
                IClient client = new RestClient(ServerlUrl, cred, new ConsoleLogger(isDebug));
                bool ok = await auth.Login(client);
                if (ok)
                {
                    runStatus = await CheckAndPrintDevices(client);
                    await auth.Logout(client);
                }
                else
                    LogError(new UftMobileException(LOGIN_FAILED));

                await SaveRunStatus(resdir, runStatus);
            }
            catch (ThreadInterruptedException e)
            {
                LogError(e, ErrorCategory.OperationStopped);
            }
        }

        private async Task<RunStatus> CheckAndPrintDevices(IClient client)
        {
            var res = await client.HttpGet<Device>(client.ServerUrl.AppendSuffix(DEVICES_ENDPOINT));
            if (res.IsOK)
            {
                if (res.Entities.Any())
                {
                    if (IncludeOfflineDevices)
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
                        return RunStatus.UNDEFINED;
                    }

                    return RunStatus.PASSED;
                }
                else
                {
                    LogError(new UftMobileException(IncludeOfflineDevices ? NO_DEVICE_FOUND : NO_AVAILABLE_DEVICE_FOUND));
                    return RunStatus.UNDEFINED;
                }
            }
            else
            {
                LogError(new UftMobileException($"StatusCode={res.StatusCode}, Error={res.Error}"));
                return RunStatus.FAILED;
            }
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

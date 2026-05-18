/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2024 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

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
        private const string NO_DEVICE_FOUND = "No device has been retrieved from the Functional Testing Lab server";
        private const string NO_AVAILABLE_DEVICE_FOUND = "No available device has been retrieved from the Functional Testing Lab server";
        private const string NO_DISCONNECTED_DEVICE_FOUND = "No disconnected device has been retrieved from the Functional Testing Lab server";
        private const string NO_APP_FOUND = "No application has been retrieved from the Functional Testing Lab server";
        private const string DEVICES_HEAD = "================================== Devices ===================================";
        private const string APPS_HEAD = "================================== Applications ==============================";
        private const string CLOUD_BROWSERS_HEAD = "================================== Cloud Browsers ==============================";
        private const string SECTION_BOTTOM = "------------------------------------------------------------------------------";
        private const string RESOURCES_BOTTOM = "==============================================================================";
        private const string GOOGLE = "https://www.google.com";
        private const string HTTP_PREFIX = "http://";
        private const string HTTPS_PREFIX = "https://";
        private const string HEAD = "HEAD";
        private const string MISSING_OR_INVALID_CREDENTIALS = "Missing or Invalid Credentials";

        private LabResxConfig _config;

        [Parameter(Position = 0, Mandatory = true)]
        public LabResxConfig Config {
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

                IWebProxy proxy = null;
                if (_config.UseProxy)
                {
                    try
                    {
                        proxy = await CheckAndGetProxy();
                    }
                    catch (WebException wex)
                    {
                        ThrowTerminatingError(new(new(GetErrorFromWebException(wex)), nameof(CheckAndGetProxy), ErrorCategory.AuthenticationError, nameof(CheckAndGetProxy)));
                    }
                    catch (Exception ex)
                    {
                        ex = ex.InnerException ?? ex;
                        string err = ex is WebException wex ? GetErrorFromWebException(wex) : $"Proxy Error: {ex.Message}";
                        WriteDebug($"{ex.GetType().Name}: {ex.Message}");
                        ThrowTerminatingError(new(new(err), nameof(CheckAndGetProxy), ErrorCategory.AuthenticationError, nameof(CheckAndGetProxy)));
                    }
                }

                IAuthenticator auth = _config.AuthType == AuthType.Basic ? new BasicAuthenticator() : new OAuth2Authenticator();
                Credentials cred = new(_config.UsernameOrClientId, _config.PasswordOrSecret, _config.TenantId);
                IClient client = new RestClient(_config.ServerUrl, cred, new ConsoleLogger(isDebug), _config.AuthType, proxy);
                bool ok = await auth.Login(client);
                if (ok)
                {
                    if (_config.Resx == Resx.CloudBrowsers)
                        runStatus = await GetAndPrintCloudBrowsers(client);
                    else
                    {
                        if (_config.Resx.In(Resx.OnlyDevices, Resx.BothDevicesAndApps))
                            runStatusDevices = await CheckAndPrintDevices(client);
                        if (_config.Resx.In(Resx.OnlyApps, Resx.BothDevicesAndApps))
                            runStatusApps = await GetAndPrintApps(client);

                        if (runStatusDevices == RunStatus.PASSED && runStatusApps == RunStatus.PASSED)
                            runStatus = RunStatus.PASSED;
                        else if (runStatusDevices == RunStatus.FAILED && runStatusApps == RunStatus.FAILED)
                            runStatus = RunStatus.FAILED;
                        else
                            runStatus = RunStatus.UNSTABLE;
                    }

                    await auth.Logout(client);

                }
                else
                    LogError(new UftMobileException(C.LOGIN_FAILED));

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
                }
                else
                {
                    WriteObject(NO_APP_FOUND);
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

        private async Task<RunStatus> GetAndPrintCloudBrowsers(IClient client)
        {
            RunStatus status = RunStatus.FAILED;
            WriteObject(CLOUD_BROWSERS_HEAD);
            var res = await client.HttpGet<CloudBrowsers>(C.BROWSERS_ENDPOINT, query: C.TOOL_VERSION, resType: ResType.Object);
            if (res.IsOK)
            {
                var data = res.Entity;
                if (data?.Browsers?.Length > 0)
                {
                    BaseWriteObject($"Available Locations ({data.Regions.Length}):");
                    data.Regions.ForEach((r, x) => BaseWriteObject($@"Location #{x} -> ""{r}"""));
                    BaseWriteObject(SECTION_BOTTOM);
                    BaseWriteObject($"Available Operating Systems ({data.OS.Length}):");
                    data.OS.ForEach((os, x) => BaseWriteObject(@$"Operating System #{x} -> ""{os}"""));
                    BaseWriteObject(SECTION_BOTTOM);
                    BaseWriteObject($"Available browsers ({data.Browsers.Length}):");
                    data.Browsers.ForEach((b, x) => BaseWriteObject($"Browser #{x} -> {b}"));
                }
                else
                {
                    WriteObject(C.NO_BROWSER_FOUND);
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
                using StreamWriter file = new(retCodeFilename, true);
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

        private async Task<IWebProxy> CheckAndGetProxy()
        {
            ProxyConfig config = _config.ProxyConfig;
            return await CheckAndGetProxy(config.ServerUrl, config.UseCredentials, config.UsernameOrClientId, config.PasswordOrSecret);
        }
        private async Task<IWebProxy> CheckAndGetProxy(string server, bool useCredentials, string username, string password)
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
            return proxy;
        }

        protected void LogError(Exception ex, ErrorCategory categ = ErrorCategory.NotSpecified, [CallerMemberName] string methodName = "")
        {
            WriteError(new ErrorRecord(ex, $"{ex.GetType()}", categ, methodName));
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
    }
}
/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2025 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Web.UI.WebControls;
using PSModule.AlmLabMgmtClient.SDK;
using PSModule.AlmLabMgmtClient.SDK.Util;
using PSModule.Common;

namespace PSModule
{
    using C = Constants;
    using L = LauncherParamsBuilder;
    using H = Helper;

    [Cmdlet(VerbsLifecycle.Invoke, "AlmLabManagementStopTask")]
    public class InvokeAlmLabManagementStopTaskCmdlet : AbstractLauncherTaskCmdlet
    {
        [Parameter(Position = 0)]
        public string BuildNumber { get; set; }

        protected override string GetRetCodeFileName()
        {
            return "StopRunReturnCode.txt";
        }

        protected override void ProcessRecord()
        {
            string kvFilePath = null, runIdFilePath = null, propsFilePath = null;
            RunStatus runStatus = RunStatus.FAILED;
            try
            {
                string ufttfsdir = Environment.GetEnvironmentVariable(UFT_LAUNCHER);
                string resDir = Path.GetFullPath(Path.Combine(ufttfsdir, $@"res\Report_{BuildNumber}"));
                string propsDir = Path.GetFullPath(Path.Combine(ufttfsdir, PROPS));
                if (Directory.Exists(resDir))
                {
                    _privateKey = H.GetPrivateKey(resDir, out kvFilePath);
                    string lastRunId = GetLastRunId(resDir);
                    string jobStatus = Environment.GetEnvironmentVariable(C.AGENT_JOBSTATUS);
                    WriteVerbose($"AGENT_JOBSTATUS = {jobStatus}");
                    if (jobStatus.EqualsIgnoreCase(C.Canceled))
                    {
                        if (lastRunId.IsNullOrEmpty())
                        {
                            WriteWarning($"Last Run ID file not found.");
                        }
                        else
                        {
                            string lastTimestamp = GetLastTimestamp(Path.Combine(resDir, C.LastTimestamp));
                            propsFilePath = Path.Combine(propsDir, $"{PROPS}{lastTimestamp}.txt");
                            if (File.Exists(propsFilePath))
                            {
                                JavaProperties ciParams = [];
                                ciParams.Load(propsFilePath);
                                if (DoStopLastRun(lastRunId, ciParams))
                                {
                                    runIdFilePath = Path.Combine(resDir, $"{lastRunId}.runid");
                                    runStatus = RunStatus.PASSED;
                                }
                            }
                            else
                            {
                                runStatus = RunStatus.FAILED;
                                WriteWarning($"Properties file not found: {propsFilePath}");
                            }
                        }
                    }
                    else
                    {
                        runStatus = RunStatus.UNDEFINED;
                    }
                }
                else
                {
                    WriteDebug($"Results directory not found: {resDir}");
                }
                CollateRetCode(resDir, (int)runStatus);
            }
            catch (AlmException ae)
            {
                LogError(ae, ae.Category);
            }
            catch (Exception e)
            {
                LogError(e);
            }
            finally
            {
                TryDeleteFile(kvFilePath);
                TryDeleteFile(runIdFilePath);
                TryDeleteFile(propsFilePath);
            }
        }

        private string GetLastTimestamp(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath).Trim();
            }
            WriteWarning($"File not found: {filePath}");
            return null;
        }

        private string GetLastRunId(string resDir)
        {
            string runIdFile = Directory.EnumerateFiles(resDir, "*.runid").Select(Path.GetFileNameWithoutExtension).FirstOrDefault();
            if (!runIdFile.IsNullOrEmpty())
            {
                string runId = Path.GetFileNameWithoutExtension(runIdFile);
                WriteVerbose($"Retrieved run ID: {runId}");
                return runId;
            }
            return null;
        }

        private bool DoStopLastRun(string runId, JavaProperties ciParams)
        {
            WriteVerbose($"Trying to stop the Run {runId} ...");
            string serverUrl = ciParams.GetOrDefault(L.ALMSERVERURL);
            string domain = ciParams.GetOrDefault(L.ALMDOMAIN);
            string project = ciParams.GetOrDefault(L.ALMPROJECT);
            bool.TryParse(ciParams.GetOrDefault(L.SSOENABLED), out bool isSSO);
            string usernameOrClientId, passwordOrApiKey;
            Aes256Encrypter aes256Encrypter = new(_privateKey);
            if (isSSO)
            {
                usernameOrClientId = ciParams.GetOrDefault(L.ALMCLIENTID);
                string encApiKeySecret = ciParams.GetOrDefault(L.ALMAPIKEYSECRET);
                passwordOrApiKey = aes256Encrypter.Decrypt(encApiKeySecret);
            }
            else
            {
                usernameOrClientId = ciParams.GetOrDefault(L.ALMUSERNAME);
                passwordOrApiKey = aes256Encrypter.Decrypt(ciParams.GetOrDefault(L.ALMPASSWORD));
            }

            string clientType = ciParams.GetOrDefault(L.CLIENTTYPE);
            WriteVerbose(C.LINE_SEP);
            WriteVerbose($"{serverUrl}, Domain: {domain}, Project: {project}, isSSO: {isSSO}, Username / ClientId: {usernameOrClientId}, ClientType: {clientType}");
            WriteVerbose(C.LINE_SEP);
            try
            {
                var runMgr = GetRunManager4Stop(runId, serverUrl, domain, project, isSSO, usernameOrClientId, passwordOrApiKey, clientType);
                if (runMgr.Login4Stop())
                {
                    if (runMgr.Stop())
                    {
                        WriteVerbose("Stop request completed.");
                        return true;
                    }
                }
                else
                {
                    WriteWarning(C.LOGIN_FAILED);
                }
            }
            catch (Exception ex)
            {
                WriteWarning($"Cleanup operation failed: {ex.Message}");
            }
            return false;
        }

        private RunManager GetRunManager4Stop(string runId, string serverUrl, string domain, string project, bool isSSO, string usernameOrClientId, string passwordOrApiKey, string clientType)
        {
            var args = new Args
            {
                Domain = domain,
                Project = project,
                ServerUrl = serverUrl,
                RunType = C.STOP_RUN,
                EntityId = runId
            };
            var cred = new Credentials(isSSO, usernameOrClientId, passwordOrApiKey);
            bool isDebug = (ActionPreference)GetVariableValue("DebugPreference") != ActionPreference.SilentlyContinue;
            var client = new RestClient(args.ServerUrl, args.Domain, args.Project, clientType, cred, new ConsoleLogger(isDebug));
            return new RunManager(client, args);
        }

        protected override Dictionary<string, string> GetTaskProperties()
        {
            throw new NotImplementedException();
        }
    }
}
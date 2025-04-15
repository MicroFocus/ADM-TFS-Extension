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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using PSModule.AlmLabMgmtClient.Result.Model;
using PSModule.AlmLabMgmtClient.SDK;
using PSModule.AlmLabMgmtClient.SDK.Util;
using PSModule.Common;

namespace PSModule
{
    using C = Constants;
    using H = Helper;

    [Cmdlet(VerbsLifecycle.Invoke, "AlmLabManagementTask")]
    public class InvokeAlmLabManagementTaskCmdlet : AbstractLauncherTaskCmdlet
    {
        private string _resDir;
        private string _runIdFilePath;

        [Parameter(Position = 0, Mandatory = true)]
        public string ALMServerPath { get; set; }

        [Parameter(Position = 1, Mandatory = false)]
        public bool IsSSO { get; set; }

        [Parameter(Position = 2)]
        public string ClientID { get; set; }

        [Parameter(Position = 3)]
        public string ApiKeySecret { get; set; }

        [Parameter(Position = 4)]
        public string ALMUserName { get; set; }

        [Parameter(Position = 5)]
        public string ALMPassword { get; set; }

        [Parameter(Position = 6, Mandatory = true)]
        public string ALMDomain { get; set; }

        [Parameter(Position = 7, Mandatory = true)]
        public string ALMProject { get; set; }

        [Parameter(Position = 8)]
        public string TestRunType { get; set; }

        [Parameter(Position = 9, Mandatory = true)]
        public string ALMEntityId { get; set; }

        [Parameter(Position = 10)]
        public string Description { get; set; }

        [Parameter(Position = 11, Mandatory = true)]
        public string TimeslotDuration { get; set; }

        [Parameter(Position = 12)]
        public string EnvironmentConfigurationID { get; set; }

        [Parameter(Position = 13)]
        public string ReportName { get; set; }

        [Parameter(Position = 14)]
        public string BuildNumber { get; set; }

        [Parameter(Position = 15)]
        public string ClientType { get; set; }

        [Parameter(Position = 16)]
        public string TimestampPattern
        {
            set
            {
                _timestampPattern = value?.Trim();
            }
        }

        protected override string GetReportFilename()
        {
            return string.IsNullOrEmpty(ReportName) ? base.GetReportFilename() : ReportName;
        }

        protected override Dictionary<string, string> GetTaskProperties()
        {
            LauncherParamsBuilder builder = new();

            builder.SetRunType(RunType.AlmLabManagement);
            builder.SetAlmServerUrl(ALMServerPath);
            builder.SetSSOEnabled(IsSSO);
            builder.SetClientID(ClientID);
            builder.SetApiKeySecret(ApiKeySecret, _privateKey);
            builder.SetAlmUserName(ALMUserName);
            builder.SetAlmPassword(ALMPassword, _privateKey);
            builder.SetAlmDomain(ALMDomain);
            builder.SetAlmProject(ALMProject);
            builder.SetClientType(ClientType);
            builder.SetBuildNumber(BuildNumber);

            switch (TestRunType)
            {
                case C.TEST_SET:
                    builder.SetTestRunType(RunTestType.TEST_SUITE);
                    break;
                case C.BVS:
                    builder.SetTestRunType(RunTestType.BUILD_VERIFICATION_SUITE);
                    break;
            }

            if (!string.IsNullOrEmpty(ALMEntityId))
            {
                int i = 1;
                foreach (string testSet in ALMEntityId.Split(C.LINE_FEED))
                {
                    builder.SetTestSet(i++, testSet.Replace(C.BACK_SLASH_, C.DOUBLE_BACK_SLASH_));
                }
            }
            else
            {
                builder.SetAlmTestSet(string.Empty);
            }

            //set ALM mandatory parameters
            builder.SetAlmTimeout(TimeslotDuration);
            builder.SetAlmRunMode(AlmRunMode.RUN_PLANNED_HOST);
            builder.SetAlmRunHost(string.Empty);

            return builder.GetProperties();
        }

        protected override string GetRetCodeFileName()
        {
            return "TestRunReturnCode.txt";
        }

        protected override void ProcessRecord()
        {
            string propsFilePath = string.Empty, resultsFilePath, lastTimestampFilePath = null;
            string kvFilePath = null;
            try
            {
                string ufttfsdir = Environment.GetEnvironmentVariable(UFT_LAUNCHER);
                string propsDir = Path.GetFullPath(Path.Combine(ufttfsdir, PROPS));

                if (!Directory.Exists(propsDir))
                    Directory.CreateDirectory(propsDir);

                _resDir = Path.GetFullPath(Path.Combine(ufttfsdir, $@"res\Report_{BuildNumber}"));
                if (!Directory.Exists(_resDir))
                    Directory.CreateDirectory(_resDir);
                DeleteExistingRunIdFiles(_resDir);

                string timestamp = DateTime.Now.ToString(DDMMYYYYHHMMSSSSS);

                propsFilePath = Path.Combine(propsDir, $"{PROPS}{timestamp}.txt");
                resultsFilePath = Path.Combine(_resDir, $"{RESULTS}{timestamp}.xml");
                _privateKey = H.GenerateAndSavePrivateKey(_resDir, out kvFilePath);

                Dictionary<string, string> properties;
                try
                {
                    properties = GetTaskProperties();
                    if (properties == null || !properties.Any())
                    {
                        throw new AlmException("Invalid or missing properties!", ErrorCategory.InvalidData);
                    }
                }
                catch (Exception e)
                {
                    ThrowTerminatingError(new ErrorRecord(e, nameof(GetTaskProperties), ErrorCategory.ParserError, nameof(ProcessRecord)));
                    return;
                }

                properties.Add(RESULTS_FILENAME, resultsFilePath.Replace(C.BACK_SLASH_, C.DOUBLE_BACK_SLASH_)); // double backslashes are expected by HpToolsLauncher.exe (JavaProperties.cs, in LoadInternal method)

                if (!SaveProperties(propsFilePath, properties))
                {
                    return;
                }

                lastTimestampFilePath = Path.Combine(_resDir, C.LastTimestamp);
                SaveTimestamp(lastTimestampFilePath, timestamp);

                //run the build task
                var runMgr = GetRunManager(_resDir);
                bool hasResults = Run(resultsFilePath, runMgr).Result;

                RunStatus runStatus = RunStatus.FAILED;
                if (hasResults)
                {
                    var listReport = H.ReadReportFromXMLFile(resultsFilePath, false, out _);
                    H.CreateSummaryReport(_resDir, RunType.AlmLabManagement, listReport, _timestampPattern);
                    //get task return code
                    runStatus = H.GetRunStatus(listReport);
                    int totalTests = H.GetNumberOfTests(listReport, out IDictionary<string, int> nrOfTests);
                    if (totalTests > 0)
                    {
                        H.CreateRunSummary(runStatus, totalTests, nrOfTests, _resDir);
                    }
                }
                CollateRetCode(_resDir, (int)runStatus);
            }
            catch (AlmException ae)
            {
                LogError(ae, ae.Category);
            }
            catch (IOException ioe)
            {
                LogError(ioe, ErrorCategory.ResourceExists);
            }
            finally
            {
                TryDeleteFile(_runIdFilePath);
                TryDeleteFile(lastTimestampFilePath);
                TryDeleteFile(kvFilePath);
                TryDeleteFile(propsFilePath);
            }
        }

        public void RunManager_RunIdGenerated(object sender, string runId)
        {
            if (string.IsNullOrEmpty(runId))
                return;

            _runIdFilePath = Path.Combine(_resDir, $"{runId}.runid");
            try
            {
                // Create an empty file, named as ###.runid pattern
                using (File.Create(_runIdFilePath)) { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating the run ID file [{_runIdFilePath}]: {ex}");
            }
        }

        private void SaveTimestamp(string filePath, string timestamp)
        {
            WriteDebug($"Saving timestamp {timestamp} to file: {filePath}");
            try
            {
                File.WriteAllText(filePath, timestamp);
            }
            catch (Exception e)
            {
                LogError(e, ErrorCategory.WriteError);
            }
        }

        private void DeleteExistingRunIdFiles(string folderPath)
        {
            try
            {
                Directory.EnumerateFiles(folderPath, "*.runid").ToList().ForEach(File.Delete);
            }
            catch (Exception ex)
            {
                WriteVerbose($"Error deleting .runid files: {ex.Message}");
            }
        }

        private async Task<bool> Run(string resFilename, RunManager runMgr)
        {
            TestSuites testsuites = await runMgr.Execute();
            if (await SaveResults(resFilename, testsuites))
            {
                return testsuites?.ListOfTestSuites.Any(ts => ts.ListOfTestCases.Any()) == true;
            }
            else
            {
                return false;
            }
        }

        private RunManager GetRunManager(string rptPath)
        {
            var args = new Args
            {
                Description = Description,
                Domain = ALMDomain,
                Project = ALMProject,
                ServerUrl = ALMServerPath,
                Duration = TimeslotDuration,
                EntityId = ALMEntityId,
                RunType = TestRunType,
                EnvironmentConfigurationId = EnvironmentConfigurationID
            };
            var cred = new Credentials(IsSSO, IsSSO ? ClientID : ALMUserName, IsSSO ? ApiKeySecret : ALMPassword);
            bool isDebug = (ActionPreference)GetVariableValue("DebugPreference") != ActionPreference.SilentlyContinue;
            var client = new RestClient(args.ServerUrl, args.Domain, args.Project, ClientType, cred, new ConsoleLogger(isDebug));
            // Create and setup RunManager
            var runManager = new RunManager(client, args, Path.Combine(rptPath, GetReportFilename()));
            runManager.RunIdGenerated += RunManager_RunIdGenerated;
            return runManager;
        }

        private async Task<bool> SaveResults(string filePath, TestSuites testsuites)
        {
            if (testsuites != null)
            {
                string xml;
                try
                {
                    xml = testsuites.ToXML();
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    LogError(e, ErrorCategory.ParserError);
                    return false;
                }
                try
                {
                    using StreamWriter file = new(filePath, true);
                    await file.WriteAsync(xml);
                    return true;
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
            return false;
        }
    }
}
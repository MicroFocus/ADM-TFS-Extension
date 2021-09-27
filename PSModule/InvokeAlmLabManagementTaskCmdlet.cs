using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PSModule.AlmLabMgmtClient.Result.Model;
using PSModule.AlmLabMgmtClient.SDK;
using PSModule.AlmLabMgmtClient.SDK.Util;

namespace PSModule
{
    using C = Constants;

    [Cmdlet(VerbsLifecycle.Invoke, "AlmLabManagementTask")]
    public class InvokeAlmLabManagementTaskCmdlet : AbstractLauncherTaskCmdlet
    {
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

        protected override string GetReportFilename()
        {
            return string.IsNullOrEmpty(ReportName) ? base.GetReportFilename() : ReportName;
        }

        public override Dictionary<string, string> GetTaskProperties()
        {
            LauncherParamsBuilder builder = new LauncherParamsBuilder();

            builder.SetRunType(RunType.AlmLabManagement);
            builder.SetAlmServerUrl(ALMServerPath);
            builder.SetSSOEnabled(IsSSO);
            builder.SetClientID(ClientID);
            builder.SetApiKeySecret(ApiKeySecret);
            builder.SetAlmUserName(ALMUserName);
            builder.SetAlmPassword(ALMPassword);
            builder.SetAlmDomain(ALMDomain);
            builder.SetAlmProject(ALMProject);
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
                    builder.SetTestSet(i++, testSet.Replace(@"\", @"\\"));
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
            string paramFileName = string.Empty, resultsFileName;
            RunManager runMgr = null;
            try
            {
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

                string ufttfsdir = Environment.GetEnvironmentVariable(UFT_LAUNCHER);
                string propdir = Path.GetFullPath(Path.Combine(ufttfsdir, PROPS));

                if (!Directory.Exists(propdir))
                    Directory.CreateDirectory(propdir);

                string resdir = Path.GetFullPath(Path.Combine(ufttfsdir, $@"res\Report_{properties[BUILD_NUMBER]}"));

                if (!Directory.Exists(resdir))
                    Directory.CreateDirectory(resdir);

                string timeSign = DateTime.Now.ToString(DDMMYYYYHHMMSSSSS);

                paramFileName = Path.Combine(propdir, $"Props{timeSign}.txt");
                resultsFileName = Path.Combine(resdir, $"Results{timeSign}.xml");

                properties.Add(RESULTS_FILENAME, resultsFileName.Replace(@"\", @"\\")); // double backslashes are expected by HpToolsLauncher.exe (JavaProperties.cs, in LoadInternal method)

                if (!SaveProperties(paramFileName, properties))
                {
                    return;
                }

                //run the build task
                runMgr = GetRunManager();
                var runStatus = Run(resultsFileName, runMgr).Result;

                //collect results
                //bool hasResults = CollateResults(resultsFileName, _launcherConsole.ToString(), resdir);

                CollateRetCode(resdir, (int)runStatus);
            }
            catch(AlmException ae)
            {
                LogError(ae, ae.Category);
                runMgr?.Stop();
            }
            catch (IOException ioe)
            {
                LogError(ioe, ErrorCategory.ResourceExists);
                runMgr?.Stop();
            }
            catch (ThreadInterruptedException e)
            {
                LogError(e, ErrorCategory.OperationStopped);
                runMgr?.Stop();
            }
        }

        private async Task<RunStatus> Run(string resFilename, RunManager runMgr)
        {
            var res = RunStatus.FAILED;
            TestSuites testsuites = await runMgr.Execute();
            await SaveResults(resFilename, testsuites).ConfigureAwait(false);
            if (testsuites?.ListOfTestSuites.Any() == true && 
                !testsuites.ListOfTestSuites.Any(ts => ts.ListOfTestCases.Any(tc => tc.Status.In(JUnitTestCaseStatus.ERROR, JUnitTestCaseStatus.FAILURE))))
            {
                res = RunStatus.PASSED;
            }
            return res;
        }

        private RunManager GetRunManager()
        {
            var args = new Args
            {
                Description = Description,
                Domain = ALMDomain,
                Project = ALMProject,
                ServerUrl = ALMServerPath,
                Duration = TimeslotDuration,
                EntityId = ALMEntityId,
                RunType = TestRunType
            };
            var cred = new Credentials(IsSSO, IsSSO ? ClientID : ALMUserName, IsSSO ? ApiKeySecret : ALMPassword);
            var client = new RestClient(args.ServerUrl, args.Domain, args.Project, ClientType, cred);
            return new RunManager(client, args);
        }

        private async Task SaveResults(string filePath, TestSuites testsuites)
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
                    return;
                }
                try
                {
                    using StreamWriter file = new StreamWriter(filePath, true);
                    await file.WriteAsync(xml).ConfigureAwait(false);
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
        }
    }
}
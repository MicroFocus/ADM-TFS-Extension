using System.Management.Automation;
using System.Linq;
using System.Collections.Generic;
using PSModule.UftMobile.SDK.UI;

namespace PSModule
{
    [Cmdlet(VerbsLifecycle.Invoke, "FSTask")]
    public class InvokeFSTaskCmdlet : AbstractLauncherTaskCmdlet
    {
        private const string MOBILE = "mobile";
        private const string WEB = "web";

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
        public bool EnableFailedTestsReport { get; set; }

        [Parameter(Position = 10)]
        public bool UseParallelRunner { get; set; }

        [Parameter(Position = 11)]
        public ParallelRunnerConfig ParallelRunnerConfig { get; set; }

        protected override bool CollateResults(string resultFile, string resdir)
        {
            return true; //do nothing here. Collate results should be made by the standard "Copy and Publish Artifacts" TFS task
        }

        public override Dictionary<string, string> GetTaskProperties()
        {
            LauncherParamsBuilder builder = new LauncherParamsBuilder();

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
            builder.SetUseParallelRunner(UseParallelRunner);
            if (UseParallelRunner)
            {
                builder.SetParallelRunnerEnvType(ParallelRunnerConfig.EnvType);
                if (ParallelRunnerConfig.EnvType == MOBILE)
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
                else if (ParallelRunnerConfig.EnvType == WEB)
                {
                    //TOOO
                }
            }

            return builder.GetProperties();
        }

        protected override string GetRetCodeFileName()
        {
            return "TestRunReturnCode.txt";
        }
    }
}

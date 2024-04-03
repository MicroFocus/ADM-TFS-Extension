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

using System.Collections.Generic;
using System.Management.Automation;

namespace PSModule
{
    using C = Common.Constants;
    [Cmdlet(VerbsLifecycle.Invoke, "AlmTask")]
    public class InvokeAlmTaskCmdlet : AbstractLauncherTaskCmdlet
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

        [Parameter(Position = 6)]
        public string ALMDomain { get; set; }

        [Parameter(Position = 7)]
        public string ALMProject { get; set; }

        [Parameter(Position = 8)]
        public string ALMTestSet { get; set; }

        [Parameter(Position = 9)]
        public string TimeOut { get; set; }

        [Parameter(Position = 10)]
        public string ReportName { get; set; }

        [Parameter(Position = 11)]
        public string RunMode { get; set; }

        [Parameter(Position = 12)]
        public string ALMRunHost { get; set; }

        [Parameter(Position = 13)]
        public string BuildNumber { get; set; }

        [Parameter(Position = 14)]
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

        public override Dictionary<string, string> GetTaskProperties()
        {
            LauncherParamsBuilder builder = new();

            builder.SetRunType(RunType.Alm);
            builder.SetAlmServerUrl(ALMServerPath);
            builder.SetSSOEnabled(IsSSO);
            builder.SetClientID(ClientID);
            builder.SetApiKeySecret(ApiKeySecret);
            builder.SetAlmUserName(ALMUserName);
            builder.SetAlmPassword(ALMPassword);
            builder.SetAlmDomain(ALMDomain);
            builder.SetAlmProject(ALMProject);
            builder.SetAlmRunHost(ALMRunHost);
            builder.SetAlmTimeout(TimeOut);
            builder.SetBuildNumber(BuildNumber);

            switch (RunMode)
            {
                case "runLocally":
                    builder.SetAlmRunMode(AlmRunMode.RUN_LOCAL);
                    break;
                case "runOnPlannedHost":
                    builder.SetAlmRunMode(AlmRunMode.RUN_PLANNED_HOST);
                    break;
                case "runRemotely":
                    builder.SetAlmRunMode(AlmRunMode.RUN_REMOTE);
                    break;
            }

            if (!string.IsNullOrEmpty(ALMTestSet))
            {
                int i = 1;
                foreach (string testSet in ALMTestSet.Split(C.LINE_FEED))
                {
                    builder.SetTestSet(i++, testSet.Replace(@"\",@"\\"));
                }
            }
            else
            {
                builder.SetAlmTestSet(string.Empty);
            }

            return builder.GetProperties();
        }

        protected override string GetRetCodeFileName()
        {
            return "TestRunReturnCode.txt";
        }

    }
}

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

using PSModule.ParallelRunner.SDK.Entity;
using System.Collections.Generic;

namespace PSModule.Models
{
    public class ReportMetaData
    {
        public string ReportPath { get; set; } //slave path of report folder(only for html report format)

        public string DisplayName { get; set; }

        public string ResourceURL { get; set; }

        public string DateTime { get; set; }

        public string Status { get; set; }

        public string Duration { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public List<TestRun> TestRuns { get; set; } = new(); // used for Parallel Runner
    }
}
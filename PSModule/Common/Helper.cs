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

using PSModule.Models;
using PSModule.ParallelRunner.SDK.Entity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;
using PRHelper = PSModule.ParallelRunner.SDK.Util.Helper;

namespace PSModule
{
    using C = Common.Constants;
    public static class Helper
    {
        #region - Private & Internal Constants

        private const string IMG_LINK_PREFIX = "https://extensionado.blob.core.windows.net/uft-extension-images";
        internal const string PASS = "pass";
        internal const string FAIL = "fail";
        internal const string ERROR = "error";
        internal const string WARNING = "warning";
        internal const string SKIPPED = "skipped";
        private const string DIEZ = "#";
        private const string TEST_REPORT_NAME_PATTERN_SUFFIX = @" - Report[\d]*$";
        private const char COLON = ':';
        private const string PACKAGE = "package";
        private const string NAME = "name";
        private const string REPORT = "report";
        private const string STATUS = "status";
        private const string TIME = "time";
        private const string START_EXEC_DT = "startExecDateTime";
        private const string FAILURE = "failure";
        private const string MESSAGE = "message";
        private const string SYSTEM_OUT = "system-out";
        private const string SYSTEM_ERR = "system-err";
        private const string TEST_NAME = "Test name";
        private const string TEST_TYPE = "Test type";
        private const string RULES = "Rules";
        private const string TIMESTAMP = "Timestamp";
        private const string FAILED_STEPS = "Failed steps";
        private const string DURATIONS = "Duration(s)";
        private const string ERROR_DETAILS = "Error details";
        private const string RUN_STATUS = "Run status";
        private const string TOTAL_TESTS = "Total tests";
        private const string _STATUS = "Status";
        private const string NO_OF_TESTS = "No. of tests";
        private const string PASSING_RATE = "Percentage of tests";
        private const string STYLE = "style";
        private const string UFT_REPORT_COL_CAPTION = "Functional Testing report";
        private const string UFT_REPORT_ARCHIVE = "Functional Testing report archive";
        private const string VIEW_REPORT = "View report";
        private const string DOWNLOAD = "Download";

        private const string LEFT = "left";
        private const string _200 = "200";
        private const string _800 = "800";
        private const string HEIGHT_30PX_AZURE = "height:30px;background-color:azure";
        private const string HEIGHT_30PX = "height:30px;";
        private const string HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200 = "font-weight:bold;min-width:200px";
        private const string HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_800 = "font-weight:bold;min-width:800px";
        private const string FONT_WEIGHT_BOLD = "font-weight:bold;";
        private const string FONT_WEIGHT_BOLD_UNDERLINE = "font-weight:bold; text-decoration:underline;";
        private const string BUILD_STATUS_IMG_STYLE = "width:50%;vertical-align:middle;";

        private const string UFT_REPORT_CAPTION = "Functional Testing Report";
        private const string RUN_SUMMARY = "Run Summary";
        private const string FAILED_TESTS = "Failed Tests";
        private const string HYPHEN = "&ndash;";

        private static readonly CultureInfo _enUS = new("en-US");

        // Alphanumeric character set: A-Z, a-z, 0-9 (62 characters)
        private const string AlphaNumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        #endregion

        public class OptionalParams
        {
            private bool _uploadArtifact;
            private ArtifactType _artifactType;
            private string _storageAccount;
            private string _container;
            private string _reportName;
            private string _archiveName;
            public OptionalParams(bool uploadArtifact, ArtifactType artifactType, string storageAccount, string container, string reportName, string archiveName)
            {
                _uploadArtifact = uploadArtifact;
                _artifactType = artifactType;
                _storageAccount = storageAccount;
                _container = container;
                _reportName = reportName;
                _archiveName = archiveName;
            }
            public bool UploadArtifact => _uploadArtifact;
            public ArtifactType ArtifactType => _artifactType;
            public string StorageAccount => _storageAccount;
            public string Container => _container;
            public string ReportName => _reportName;
            public string ArchiveName => _archiveName;
        }

        public static IList<ReportMetaData> ReadReportFromXMLFile(string reportPath, bool isJUnitReport, out IList<KeyValuePair<string, IList<ReportMetaData>>> failedSteps, bool addParallelTestRuns = false)
        {
            failedSteps = [];
            IList<ReportMetaData> listReport = [];
            XmlDocument xmlDoc = new();
            xmlDoc.Load(reportPath);

            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes) //inside <testsuite> node 
            {
                IList<ReportMetaData> failedTestSteps = [];
                string testName = string.Empty;
                if (isJUnitReport)
                {
                    string currentTestAndReportName = node.Attributes[PACKAGE].Value;
                    testName = Regex.Replace(currentTestAndReportName, TEST_REPORT_NAME_PATTERN_SUFFIX, string.Empty);
                }

                foreach (XmlNode currentNode in node) //inside <testcase> nodes
                {
                    ReportMetaData reportmetadata = new();
                    var attributesList = currentNode.Attributes;
                    foreach (XmlAttribute attribute in attributesList)
                    {
                        switch (attribute.Name)
                        {
                            case NAME: reportmetadata.DisplayName = attribute.Value; break;
                            case REPORT: reportmetadata.ReportPath = attribute.Value; break;
                            case STATUS: reportmetadata.Status = attribute.Value; break;
                            case TIME: reportmetadata.Duration = attribute.Value; break;
                            case START_EXEC_DT: reportmetadata.DateTime = attribute.Value; break;
                            default: break;
                        }
                    }

                    if (isJUnitReport)
                    {
                        //remove the number in front of each step
                        string stepName = reportmetadata.DisplayName;
                        if (stepName?.StartsWith(DIEZ) == true)
                        {
                            reportmetadata.DisplayName = stepName.Substring(stepName.IndexOf(COLON) + 1);
                        }
                    }

                    var nodes = currentNode.ChildNodes;
                    foreach (XmlNode xmlNode in nodes)//inside nodes in <testcase> nodes
                    {
                        if (xmlNode.Name.In(FAILURE, ERROR))
                        {
                            foreach (XmlAttribute attribute in xmlNode.Attributes)
                            {
                                if (attribute.Name == MESSAGE && !attribute.Value.IsNullOrWhiteSpace())
                                {
                                    if (reportmetadata.ErrorMessage.IsNullOrWhiteSpace())
                                    {
                                        reportmetadata.ErrorMessage = attribute.Value;
                                    }
                                    else
                                    {
                                        reportmetadata.ErrorMessage += $"<br>{attribute.Value}";
                                    }

                                    if (reportmetadata.Status.IsNullOrWhiteSpace())
                                    {
                                        reportmetadata.Status = xmlNode.Name == FAILURE ? FAIL : ERROR;
                                    }
                                    break;
                                }
                            }
                        }
                        else if (xmlNode.Name == SYSTEM_OUT && reportmetadata.DateTime.IsNullOrWhiteSpace() && reportmetadata.Status != SKIPPED)
                        {
                            reportmetadata.DateTime = xmlNode.InnerText.Substring(0, 19);
                        }
                        else if (xmlNode.Name == SYSTEM_ERR && xmlNode.InnerText.IsNullOrWhiteSpace())
                        {
                            if (reportmetadata.ErrorMessage.IsNullOrWhiteSpace())
                            {
                                reportmetadata.ErrorMessage = $"{xmlNode.InnerText.Trim()}";
                            }
                            else
                            {
                                reportmetadata.ErrorMessage += $"<br>{xmlNode.InnerText.Trim()}";
                            }

                        }
                    }
                    if (isJUnitReport && reportmetadata.Status == FAIL)
                    {
                        failedTestSteps.Add(reportmetadata);
                    }
                    if (addParallelTestRuns && !reportmetadata.ReportPath.IsNullOrEmpty())
                    {
                        reportmetadata.TestRuns = PRHelper.GetTestRuns(reportmetadata.ReportPath);
                    }
                    listReport.Add(reportmetadata);
                }
                if (isJUnitReport && failedTestSteps.Any())
                {
                    failedSteps.Add(new(testName, failedTestSteps));
                }
            }

            return listReport;
        }

        public static RunStatus GetRunStatus(IList<ReportMetaData> listReport)
        {
            var runStatus = RunStatus.PASSED;
            int passedTests = 0, failedTests = 0;

            foreach (ReportMetaData report in listReport)
            {
                if (report.Status == PASS)
                {
                    passedTests++;
                }
                else if (report.Status.In(ERROR, FAIL))
                {
                    failedTests++;
                }
            }

            if (passedTests > 0 && failedTests > 0)
            {
                runStatus = RunStatus.UNSTABLE;
            }
            else if (passedTests == 0 && failedTests > 0)
            {
                runStatus = RunStatus.FAILED;
            }

            return runStatus;
        }

        public static int GetNumberOfTests(IList<ReportMetaData> listReport, out IDictionary<string, int> nrOfTests)
        {
            nrOfTests = new Dictionary<string, int>
            {
                { PASS, 0 },
                { FAIL, 0 },
                { ERROR, 0 },
                { WARNING, 0 },
                { SKIPPED, 0 }
            };

            int nrOfTestsCount = 0;
            foreach (ReportMetaData item in listReport)
            {
                if (item.TestRuns.Any())
                {
                    foreach (var testRun in item.TestRuns)
                    {
                        nrOfTests[testRun.GetAzureStatus()]++;
                    }
                    nrOfTestsCount += item.TestRuns.Count;
                }
                else
                {
                    nrOfTests[item.Status]++;
                    nrOfTestsCount++;
                }
            }

            return nrOfTestsCount;
        }

        public static void CreateSummaryReport(string rptPath, RunType runType, IList<ReportMetaData> reportList, string tsPattern, OptionalParams optParams = null)
        {
            bool uploadArtifact = false;
            ArtifactType artifactType = ArtifactType.None;
            string storageAccount = string.Empty, container = string.Empty, reportName = string.Empty, archiveName = string.Empty;
            if (optParams != null)
            {
                uploadArtifact = optParams.UploadArtifact;
                artifactType = optParams.ArtifactType;
                storageAccount = optParams.StorageAccount;
                container = optParams.Container;
                reportName = optParams.ReportName;
                archiveName = optParams.ArchiveName;
            }

            HtmlTable table = new();
            HtmlTableRow header = new();
            HtmlTableCell cell;
            cell = new() { InnerText = TEST_NAME, Width = _200, Align = LEFT };
            cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(cell);

            cell = new() { InnerText = TIMESTAMP, Width = _200, Align = LEFT };
            cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(cell);

            cell = new() { InnerText = _STATUS, Width = _200, Align = LEFT };
            cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(cell);

            if (runType == RunType.FileSystem && uploadArtifact)
            {
                if (artifactType.In(ArtifactType.onlyReport, ArtifactType.bothReportArchive))
                {
                    cell = new() { InnerText = UFT_REPORT_COL_CAPTION, Width = _200, Align = LEFT };
                    cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
                    header.Cells.Add(cell);
                }
                if (artifactType.In(ArtifactType.onlyArchive, ArtifactType.bothReportArchive))
                {
                    cell = new() { InnerText = UFT_REPORT_ARCHIVE, Width = _200, Align = LEFT };
                    cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
                    header.Cells.Add(cell);
                }
            }

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            //create table content
            int index = 1;
            var linkPrefix = $"https://{storageAccount}.blob.core.windows.net/{container}";
            var zipLinkPrefix = $"{linkPrefix}/{archiveName}";
            var htmlLinkPrefix = $"{linkPrefix}/{reportName}";
            foreach (ReportMetaData report in reportList)
            {
                var row = new HtmlTableRow();
                cell = new() { InnerText = GetTestName(report.DisplayName), Align = LEFT };
                row.Cells.Add(cell);

                cell = new() { Align = LEFT };
                if (report.DateTime.IsNullOrEmpty())
                {
                    cell.Controls.Add(new LiteralControl(HYPHEN));
                }
                else
                {
                    if (tsPattern.IsNullOrWhiteSpace())
                    {
                        tsPattern = C.DEFAULT_DT_PATTERN;
                    }
                    if (DateTime.TryParseExact(report.DateTime, C.DEFAULT_DT_PATTERN, _enUS, DateTimeStyles.None, out DateTime dt))
                    {
                        try
                        {
                            cell.InnerText = dt.ToString(tsPattern);
                        }
                        catch
                        {
                            cell.InnerText = report.DateTime;
                        }
                    }
                    else
                    {
                        cell.InnerText = report.DateTime;
                    }
                }
                row.Cells.Add(cell);

                cell = new() { Align = LEFT };
                cell.Controls.Add(new HtmlImage { Src = $"{IMG_LINK_PREFIX}/{report.Status}.svg" });
                row.Cells.Add(cell);

                if (runType == RunType.FileSystem && uploadArtifact)
                {
                    if (report.ReportPath.IsNullOrWhiteSpace())
                    {
                        cell = new() { Align = LEFT };
                        cell.Controls.Add(new LiteralControl(HYPHEN));
                        row.Cells.Add(cell);
                        if (artifactType == ArtifactType.bothReportArchive)
                        {
                            cell = new() { Align = LEFT };
                            cell.Controls.Add(new LiteralControl(HYPHEN));
                            row.Cells.Add(cell);
                        }
                    }
                    else
                    {
                        if (artifactType.In(ArtifactType.onlyReport, ArtifactType.bothReportArchive))
                        {
                            row.Cells.Add(GetNewRptLinkCell($"{htmlLinkPrefix}_{index}.html"));
                        }
                        if (artifactType.In(ArtifactType.onlyArchive, ArtifactType.bothReportArchive))
                        {
                            row.Cells.Add(GetNewRptLinkCell($"{zipLinkPrefix}_{index}.zip", false));
                        }
                        index++;
                    }
                }
                table.Rows.Add(row);
            }

            //add table to file
            using StringWriter sw = new();
            table.RenderControl(new HtmlTextWriter(sw));
            string html = sw.ToString();
            File.WriteAllText(Path.Combine(rptPath, UFT_REPORT_CAPTION), html);
        }

        public static void CreateParallelSummaryReport(string rptPath, RunType runType, IList<ReportMetaData> reportList, string tsPattern, OptionalParams optParams = null)
        {
            bool uploadArtifact = false;
            ArtifactType artifactType = ArtifactType.None;
            string storageAccount = string.Empty, container = string.Empty, reportName = string.Empty, archiveName = string.Empty;
            if (optParams != null)
            {
                uploadArtifact = optParams.UploadArtifact;
                artifactType = optParams.ArtifactType;
                storageAccount = optParams.StorageAccount;
                container = optParams.Container;
                reportName = optParams.ReportName;
                archiveName = optParams.ArchiveName;
            }

            HtmlTable table = new();
            HtmlTableRow header = new();
            HtmlTableCell cell;
            cell = new() { InnerText = TEST_NAME, Width = _200, Align = LEFT };
            cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(cell);

            cell = new() { InnerText = TEST_TYPE, Width = _200, Align = LEFT };
            cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(cell);

            cell = new() { InnerText = RULES, Width = _200, Align = LEFT };
            cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(cell);

            cell = new() { InnerText = TIMESTAMP, Width = _200, Align = LEFT };
            cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(cell);

            cell = new() { InnerText = _STATUS, Width = _200, Align = LEFT };
            cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(cell);

            if (uploadArtifact)
            {
                if (artifactType.In(ArtifactType.onlyReport, ArtifactType.bothReportArchive))
                {
                    cell = new() { InnerText = UFT_REPORT_COL_CAPTION, Width = _200, Align = LEFT };
                    cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
                    header.Cells.Add(cell);
                }
                if (artifactType.In(ArtifactType.onlyArchive, ArtifactType.bothReportArchive))
                {
                    cell = new() { InnerText = UFT_REPORT_ARCHIVE, Width = _200, Align = LEFT };
                    cell.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
                    header.Cells.Add(cell);
                }
            }

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            //create table content
            int index = 1;
            var linkPrefix = $"https://{storageAccount}.blob.core.windows.net/{container}";
            var zipLinkPrefix = $"{linkPrefix}/{archiveName}";
            var htmlLinkPrefix = $"{linkPrefix}/{reportName}";
            foreach (ReportMetaData report in reportList)
            {
                int x = 1;
                foreach (TestRun testRun in report.TestRuns)
                {
                    string timestamp = testRun.RunStartTime.IsNullOrEmpty() ? report.DateTime : testRun.RunStartTime;
                    if (tsPattern.IsNullOrWhiteSpace())
                    {
                        tsPattern = C.DEFAULT_DT_PATTERN;
                    }
                    if (DateTime.TryParseExact(timestamp, C.DEFAULT_DT_PATTERN, _enUS, DateTimeStyles.None, out DateTime dt))
                    {
                        try
                        {
                            timestamp = dt.ToString(tsPattern);
                        }
                        catch { }
                    }

                    HtmlTableRow row = new();
                    row.Cells.Add(new() { InnerText = $"{testRun.Name} [{x}]", Align = LEFT });
                    row.Cells.Add(new() { InnerText = $"{testRun.GetEnvType()}", Align = LEFT });
                    row.Cells.Add(new() { InnerHtml = testRun.GetDetails(), Align = LEFT });
                    row.Cells.Add(new() { InnerText = timestamp, Align = LEFT });

                    cell = new() { Align = LEFT };
                    cell.Controls.Add(new HtmlImage { Src = $"{IMG_LINK_PREFIX}/{testRun.GetAzureStatus()}.svg" });
                    row.Cells.Add(cell);

                    if (runType == RunType.FileSystem && uploadArtifact && testRun.HasUFTReport(report.ReportPath))
                    {
                        if (artifactType.In(ArtifactType.onlyReport, ArtifactType.bothReportArchive))
                        {
                            row.Cells.Add(GetNewRptLinkCell($"{htmlLinkPrefix}_{index}.html"));
                        }
                        if (artifactType.In(ArtifactType.onlyArchive, ArtifactType.bothReportArchive))
                        {
                            row.Cells.Add(GetNewRptLinkCell($"{zipLinkPrefix}_{index}.zip", false));
                        }
                        index++;
                    }
                    x++;
                    table.Rows.Add(row);
                }
            }

            //add table to file
            StringWriter sw = new();
            table.RenderControl(new(sw));
            string html = sw.ToString();
            File.WriteAllText(Path.Combine(rptPath, UFT_REPORT_CAPTION), html);
        }

        private static HtmlTableCell GetNewRptLinkCell(string href, bool isHtmlLink = true)
        {
            HtmlTableCell cell = new() { Align = LEFT };
            HtmlAnchor a = new() { HRef = href, InnerText = (isHtmlLink ? VIEW_REPORT : DOWNLOAD) };
            cell.Controls.Add(a);
            return cell;
        }

        public static void CreateRunSummary(RunStatus runStatus, int totalTests, IDictionary<string, int> nrOfTests, string rptPath)
        {
            HtmlTable table = new();
            HtmlTableRow header = new();

            HtmlTableCell h1 = new() { InnerText = RUN_STATUS, Width = _200, Align = LEFT };
            h1.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h1);

            HtmlTableCell h2 = new() { InnerText = TOTAL_TESTS, Width = _200, Align = LEFT };
            h2.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h2);

            HtmlTableCell h3 = new() { InnerText = _STATUS, Width = _200, Align = LEFT, ColSpan = 2 };
            h3.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h3);

            HtmlTableCell h4 = new() { InnerText = NO_OF_TESTS, Width = _200, Align = LEFT };
            h4.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h4);

            HtmlTableCell h5 = new() { InnerText = PASSING_RATE, Width = _200, Align = LEFT };
            h5.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h5);

            header.BgColor = KnownColor.Azure.ToString();
            table.Rows.Add(header);

            string[] statuses = nrOfTests.Keys.ToArray();
            int length = statuses.Length;

            var percentages = new decimal[length];
            for (int index = 0; index < length; index++)
            {
                percentages[index] = (decimal)(100 * nrOfTests[statuses[index]]) / totalTests;
            }
            var roundedPercentages = GetPerfectRounding(percentages);
            //create table content
            for (int index = 0; index < length; index++)
            {
                HtmlTableRow row = new();
                if (index == 0)
                {
                    HtmlTableCell cell1 = new() { Align = LEFT, RowSpan = 5 };
                    HtmlImage img = new() { Src = $"{IMG_LINK_PREFIX}/build_status/{runStatus.ToString().ToLower()}.svg" };
                    img.Attributes.Add(STYLE, BUILD_STATUS_IMG_STYLE);
                    cell1.Controls.Add(img);

                    row.Cells.Add(cell1);

                    HtmlTableCell cell2 = new() { InnerText = $"{totalTests}", Align = LEFT, RowSpan = 5 };
                    cell2.Attributes.Add(STYLE, FONT_WEIGHT_BOLD);
                    row.Cells.Add(cell2);
                }

                HtmlTableCell cell3 = new() { Align = LEFT };
                HtmlImage statusImage = new()
                {
                    Src = $"{IMG_LINK_PREFIX}/{statuses[index].ToLower()}.svg"
                };
                cell3.Controls.Add(statusImage);
                row.Cells.Add(cell3);

                row.Cells.Add(new() { Align = LEFT, InnerText = statuses[index] });
                row.Cells.Add(new() { Align = LEFT, InnerText = nrOfTests[statuses[index]].ToString() });
                row.Cells.Add(new() { Align = LEFT, InnerText = $"{roundedPercentages[index]:0.00}%" });

                row.Attributes.Add(STYLE, HEIGHT_30PX);
                table.Rows.Add(row);
            }

            //add table to file
            using StringWriter sw = new();
            table.RenderControl(new(sw));
            string html = sw.ToString();
            File.WriteAllText(Path.Combine(rptPath, RUN_SUMMARY), html);
        }

        public static void CreateFailedStepsReport(IList<KeyValuePair<string, IList<ReportMetaData>>> failedSteps, string rptPath)
        {
            if (failedSteps.IsNullOrEmpty())
                return;

            HtmlTable table = new();
            HtmlTableRow header = new();

            HtmlTableCell h1 = new() { InnerText = TEST_NAME, Width = _200, Align = LEFT };
            h1.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h1);

            HtmlTableCell h2 = new() { InnerText = FAILED_STEPS, Width = _200, Align = LEFT };
            h2.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h2);

            HtmlTableCell h3 = new() { InnerText = DURATIONS, Width = _200, Align = LEFT };
            h3.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_200);
            header.Cells.Add(h3);

            HtmlTableCell h4 = new() { InnerText = ERROR_DETAILS, Width = _800, Align = LEFT };
            h4.Attributes.Add(STYLE, HDR_FONT_WEIGHT_BOLD_MIN_WIDTH_800);
            header.Cells.Add(h4);

            header.Attributes.Add(STYLE, HEIGHT_30PX);
            table.Rows.Add(header);

            bool isOddRow = true;
            foreach (var kvp in failedSteps)
            {
                string testName = kvp.Key;
                var failedTestSteps = kvp.Value;
                int index = 0;
                string style = isOddRow ? HEIGHT_30PX_AZURE : HEIGHT_30PX;
                foreach (var item in failedTestSteps)
                {
                    HtmlTableRow row = new();
                    if (index == 0)
                    {
                        HtmlTableCell cell1 = new() { InnerText = testName, Align = LEFT };
                        cell1.Attributes.Add(STYLE, FONT_WEIGHT_BOLD_UNDERLINE);
                        cell1.RowSpan = failedTestSteps.Count;
                        row.Cells.Add(cell1);
                    }

                    row.Cells.Add(new() { InnerText = item.DisplayName, Align = LEFT });
                    row.Cells.Add(new() { InnerText = item.Duration, Align = LEFT });
                    row.Cells.Add(new() { InnerText = item.ErrorMessage, Align = LEFT });

                    row.Attributes.Add(STYLE, style);
                    table.Rows.Add(row);

                    index++;
                }
                if (failedTestSteps.Any())
                    isOddRow = !isOddRow;
            }

            //add table to file
            using StringWriter sw = new();
            table.RenderControl(new(sw));
            string html = sw.ToString();
            File.WriteAllText(Path.Combine(rptPath, FAILED_TESTS), html);
        }

        private static string GetTestName(string testPath)
        {
            int pos = testPath.LastIndexOf(C.BACK_SLASH_, StringComparison.Ordinal) + 1;
            return testPath.Substring(pos, testPath.Length - pos);
        }

        private static string ImageToBase64(Image _imagePath)
        {
            byte[] imageBytes = ImageToByteArray(_imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }

        private static byte[] ImageToByteArray(Image imageIn)
        {
            MemoryStream ms = new();
            imageIn.Save(ms, ImageFormat.Png);

            return ms.ToArray();
        }

        private static decimal[] GetPerfectRounding(decimal[] original, decimal expectedSum = 100, int decimals = 1)
        {
            var rounded = original.Select(x => Math.Round(x, decimals)).ToArray();
            var delta = expectedSum - rounded.Sum();
            if (delta == 0) return rounded;
            var deltaUnit = Convert.ToDecimal(Math.Pow(0.1, decimals)) * Math.Sign(delta);

            IList<int> applyDeltaSequence;
            if (delta < 0)
            {
                applyDeltaSequence = original
                    .Zip(Enumerable.Range(0, int.MaxValue), (x, index) => new { x, index })
                    .OrderBy(a => original[a.index] - rounded[a.index])
                    .ThenByDescending(a => a.index)
                    .Select(a => a.index).ToList();
            }
            else
            {
                applyDeltaSequence = original
                    .Zip(Enumerable.Range(0, int.MaxValue), (x, index) => new { x, index })
                    .OrderByDescending(a => original[a.index] - rounded[a.index])
                    .Select(a => a.index).ToList();
            }

            Enumerable.Repeat(applyDeltaSequence, int.MaxValue)
                .SelectMany(x => x)
                .Take(Convert.ToInt32(delta / deltaUnit))
                .ForEach(index => rounded[index] += deltaUnit);

            return rounded;
        }

        public static SecureString GetPrivateKey(string resDir, out string filePath)
        {
            filePath = Path.Combine(resDir, C._RANDOM_KEY_TMP);
            if (File.Exists(filePath))
            {
                return GetKeyFromFile(filePath);
            }
            else
            {
                filePath = null;
            }
            return null;
        }

        public static SecureString GenerateAndSavePrivateKey(string path, out string filePath)
        {
            filePath = Path.Combine(path, C._RANDOM_KEY_TMP);
            var key = GenerateAlphaNumericSecureString(32);
            SaveKeyToProtectedHiddenFile(filePath, key);
            return key;
        }

        public static string GenerateAlphaNumericString(int length)
        {
            using var rng = RandomNumberGenerator.Create();
            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                byte[] buffer = new byte[1];
                rng.GetBytes(buffer);
                int index = buffer[0] % AlphaNumericChars.Length;
                result.Append(AlphaNumericChars[index]);
            }
            return result.ToString();
        }

        public static void SaveKeyToProtectedHiddenFile(string filePath, SecureString secureKey)
        {
            // Convert SecureString to byte arrays
            byte[] keyBytes = secureKey.ToByteArray();

            // Encrypt the byte arrays using DPAPI for the current user
            byte[] keyEncrypted = ProtectedData.Protect(keyBytes, null, DataProtectionScope.CurrentUser);

            // Write the encrypted data to the file
            using (FileStream fs = new(filePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new(fs))
            {
                // Write the key: length followed by encrypted bytes
                writer.Write(keyEncrypted.Length);
                writer.Write(keyEncrypted);
            }

            // Set the file attributes to hidden
            File.SetAttributes(filePath, FileAttributes.Hidden);
            // Apply strict ACLs
            SetFileAcl(filePath);
        }

        private static void SetFileAcl(string filePath)
        {
            // Get the current user's identity
            string currentUser = WindowsIdentity.GetCurrent().Name;

            // Create a FileSecurity object
            FileSecurity fileSecurity = new();

            // Disable inheritance and remove inherited rules
            fileSecurity.SetAccessRuleProtection(true, false);

            // Grant full control to the current user only
            FileSystemAccessRule accessRule = new(
                currentUser,
                FileSystemRights.FullControl,
                AccessControlType.Allow
            );

            // Add the access rule
            fileSecurity.AddAccessRule(accessRule);

            // Apply the ACLs to the file
            File.SetAccessControl(filePath, fileSecurity);
        }

        private static SecureString GetKeyFromFile(string filePath)
        {
            try
            {
                // Read the encrypted data from the file
                byte[] keyEncrypted;
                using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new(fs))
                {
                    // Read the length of the encrypted key (4 bytes), then the key itself
                    int keyLength = reader.ReadInt32();
                    keyEncrypted = reader.ReadBytes(keyLength);
                }

                // Decrypt the data using DPAPI for the current user
                byte[] keyBytes = ProtectedData.Unprotect(keyEncrypted, null, DataProtectionScope.CurrentUser);

                // Convert decrypted byte arrays to SecureString
                SecureString secretKey = keyBytes.ToSecureString();

                // Return the KeyVector object
                return secretKey;
            }
            catch (FileNotFoundException ex)
            {
                throw new Exception("File not found: " + ex.Message, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new Exception("Access denied to file: " + ex.Message, ex);
            }
            catch (CryptographicException ex)
            {
                throw new Exception("Decryption failed: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred: " + ex.Message, ex);
            }
        }

        private static SecureString GenerateAlphaNumericSecureString(int count)
        {
            string str = GenerateAlphaNumericString(count);
            return str.ToSecureString();
        }
    }
}

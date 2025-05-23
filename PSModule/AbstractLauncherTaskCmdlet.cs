﻿/*
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
using System.IO;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Concurrent;
using PSModule.Models;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using PSModule.UftMobile.SDK.UI;
using System.Security;

namespace PSModule
{
    using H = Helper;
    using H2 = ParallelRunner.SDK.Util.Helper;
    using C = Common.Constants;

    public abstract class AbstractLauncherTaskCmdlet : PSCmdlet
    {
        #region - Private Constants

        private const string FTToolsLauncher_EXE = "FTToolsLauncher.exe";
        private const string ReportConverter_EXE = "ReportConverter.exe";
        protected const string UFT_LAUNCHER = "UFT_LAUNCHER";
        protected const string PROPS = "props";
        protected const string RESULTS = "Results";
        protected const string BUILD_NUMBER = "buildNumber";
        protected const string DDMMYYYYHHMMSSSSS = "ddMMyyyyHHmmssSSS";
        protected const string RESULTS_FILENAME = "resultsFilename";
        private const string STORAGE_ACCOUNT = "storageAccount";
        private const string CONTAINER = "container";
        protected const string RUN_TYPE = "runType";
        private const string UPLOAD_ARTIFACT = "uploadArtifact";
        private const string ARTIFACT_TYPE = "artifactType";
        private const string REPORT_NAME = "reportName";
        private const string ARCHIVE_NAME = "archiveName";
        protected const string YES = "yes";
        private const string JUNIT_REPORT_XML = "junit_report.xml";

        #endregion

        private readonly StringBuilder _launcherConsole = new();
        private readonly ConcurrentQueue<string> _errorToProcess = new();

        protected bool _enableFailedTestsReport;
        protected bool _generateJUnitReport;
        protected bool _isParallelRunnerMode;
        protected List<string> _rptPaths; // this field is instanciated in RunFromFileSystemTask\localTask.ps1 or ParallelRunnerTask\localTask.ps1 and passed to / filled in InvokeFSTaskCmdlet, then read in localTask.ps1
        protected ServerConfigEx _dlServerConfig;
        protected DeviceConfig _deviceConfig;
        protected CloudBrowserConfig _cloudBrowserConfig;
        protected ParallelRunnerConfig _parallelRunnerConfig;
        protected string _timestampPattern;
        protected SecureString _privateKey;

        protected AbstractLauncherTaskCmdlet() { }

        protected abstract Dictionary<string, string> GetTaskProperties();

        private delegate void CreateSummaryReport(string rptPath, RunType runType, IList<ReportMetaData> reportList, string tsPattern, H.OptionalParams optionalParams = null);

        protected override void ProcessRecord()
        {
            string launcherPath, converterPath, propsFilePath = string.Empty, resultsFilePath;
            try
            {
                Dictionary<string, string> properties;
                try
                {
                    properties = GetTaskProperties();
                    if (properties.IsNullOrEmpty())
                    {
                        ThrowTerminatingError(new ErrorRecord(new Exception("Invalid or missing properties!"), nameof(GetTaskProperties), ErrorCategory.InvalidData, nameof(GetTaskProperties)));
                        return;
                    }
                }
                catch (Exception e)
                {
                    ThrowTerminatingError(new ErrorRecord(e, nameof(GetTaskProperties), ErrorCategory.ParserError, nameof(GetTaskProperties)));
                    return;
                }

                string ufttfsdir = Environment.GetEnvironmentVariable(UFT_LAUNCHER);

                launcherPath = Path.GetFullPath(Path.Combine(ufttfsdir, FTToolsLauncher_EXE));
                converterPath = Path.GetFullPath(Path.Combine(ufttfsdir, ReportConverter_EXE));

                string propsDir = Path.GetFullPath(Path.Combine(ufttfsdir, PROPS));

                if (!Directory.Exists(propsDir))
                    Directory.CreateDirectory(propsDir);
                if (!properties.ContainsKey(BUILD_NUMBER))
                {
                    LogError(new InvalidDataException("Missing buildNumber property!"), ErrorCategory.InvalidData);
                    return;
                }
                string resdir = Path.GetFullPath(Path.Combine(ufttfsdir, $@"res\Report_{properties[BUILD_NUMBER]}"));

                if (!Directory.Exists(resdir))
                    Directory.CreateDirectory(resdir);

                string timeSign = DateTime.Now.ToString(DDMMYYYYHHMMSSSSS);

                propsFilePath = Path.Combine(propsDir, $"{PROPS}{timeSign}.txt");
                resultsFilePath = Path.Combine(resdir, $"{RESULTS}{timeSign}.xml");

                properties.Add(RESULTS_FILENAME, resultsFilePath.Replace(C.BACK_SLASH_, C.DOUBLE_BACK_SLASH_)); // double backslashes are expected by HpToolsLauncher.exe (JavaProperties.cs, in LoadInternal method)

                if (!SaveProperties(propsFilePath, properties))
                {
                    return;
                }
                //run the build task
                var exitCode = Run(launcherPath, propsFilePath);
                var runType = (RunType)Enum.Parse(typeof(RunType), properties[RUN_TYPE]);
                bool hasResults = HasResults(resultsFilePath, out string xmlResults);
                if (!hasResults)
                {
                    ErrorCategory categ = exitCode == LauncherExitCode.AlmNotConnected ? ErrorCategory.ConnectionError : ErrorCategory.InvalidData;
                    if (_errorToProcess.TryDequeue(out string error))
                    {
                        ThrowTerminatingError(new ErrorRecord(new Exception(error), nameof(ProcessRecord), categ, nameof(ProcessRecord)));
                    }
                    CollateRetCode(resdir, (int)exitCode);
                }
                else
                {
                    CreateSummaryReport createSummaryReportHandler = _isParallelRunnerMode ? H.CreateParallelSummaryReport : H.CreateSummaryReport;
                    RunStatus runStatus = RunStatus.FAILED;
                    if (CollateResults(xmlResults, resdir))
                    {
                        var listReport = H.ReadReportFromXMLFile(resultsFilePath, false, out _, _isParallelRunnerMode);
                        //create html report
                        if (runType == RunType.FileSystem && properties[UPLOAD_ARTIFACT] == YES)
                        {
                            string storageAccount = properties.GetValueOrDefault(STORAGE_ACCOUNT, string.Empty);
                            string container = properties.GetValueOrDefault(CONTAINER, string.Empty);
                            var artifactType = (ArtifactType)Enum.Parse(typeof(ArtifactType), properties[ARTIFACT_TYPE]);
                            var optParams = new H.OptionalParams(true, artifactType, storageAccount, container, properties[REPORT_NAME], properties[ARCHIVE_NAME]);
                            createSummaryReportHandler(resdir, runType, listReport, _timestampPattern, optParams);
                        }
                        else
                        {
                            createSummaryReportHandler(resdir, runType, listReport, _timestampPattern);
                        }
                        //get task return code
                        runStatus = exitCode == LauncherExitCode.Closed ? RunStatus.CANCELED : H.GetRunStatus(listReport);
                        int totalTests = H.GetNumberOfTests(listReport, out IDictionary<string, int> nrOfTests);
                        if (totalTests > 0)
                        {
                            H.CreateRunSummary(runStatus, totalTests, nrOfTests, resdir);
                            if (runType == RunType.FileSystem)
                            {
                                if (listReport.Any())
                                {
                                    var validTestCases = listReport.Where(t => !t.ReportPath.IsNullOrWhiteSpace());
                                    if (validTestCases.Any())
                                    {
                                        if (_isParallelRunnerMode)
                                        {
                                            foreach (var tc in validTestCases)
                                            {
                                                IOrderedEnumerable<string> dirs;
                                                if (tc.AreTestRunsFromJsonRpt())
                                                {
                                                    dirs = tc.TestRuns.Where(tr => H2.HasUftHtmlReport(tr.Path)).Select(tr => @$"{tr.Path}\Report").OrderBy(d => d);
                                                }
                                                else
                                                {
                                                    dirs = new DirectoryInfo(tc.ReportPath).GetFiles(C.RUN_RESULTS_XML, SearchOption.AllDirectories).Select(f => f.Directory.FullName).OrderBy(d => d);
                                                }
                                                if (dirs.Any())
                                                {
                                                    _rptPaths.AddRange(dirs);
                                                }
                                                else
                                                {
                                                    LogWarning($"The report file '{C.RUN_RESULTS_XML}' is not found in '{tc.ReportPath}'.");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            validTestCases.ForEach(tc => _rptPaths.Add(tc.ReportPath));
                                        }
                                    }
                                }
                                if (runStatus != RunStatus.CANCELED && _rptPaths.Any())
                                {
                                    //run junit report converter
                                    if (_generateJUnitReport)
                                    {
                                        string outputFileReport = Path.Combine(resdir, JUNIT_REPORT_XML);
                                        RunConverter(converterPath, outputFileReport);
                                        if (_enableFailedTestsReport && nrOfTests[H.FAIL] > 0 &&
                                            File.Exists(outputFileReport) && new FileInfo(outputFileReport).Length > 0)
                                        {
                                            H.ReadReportFromXMLFile(outputFileReport, true,
                                                out IList<KeyValuePair<string, IList<ReportMetaData>>> failedSteps);
                                            H.CreateFailedStepsReport(failedSteps, resdir);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (_errorToProcess.TryDequeue(out string error) && !error.StartsWith(C.LAUNCHER_EXITED_WITH_CODE))
                    {
                        ThrowTerminatingError(new ErrorRecord(new Exception(error), nameof(ProcessRecord), ErrorCategory.InvalidData, nameof(ProcessRecord)));
                    }
                    CollateRetCode(resdir, (int)runStatus);
                }
            }
            catch (IOException ioe)
            {
                LogError(ioe);
            }
        }

        protected bool SaveProperties(string paramsFile, Dictionary<string, string> properties)
        {
            bool result = true;

            using var file = new StreamWriter(paramsFile, true);
            try
            {
                foreach (string prop in properties.Keys.ToArray())
                {
                    file.WriteLine($"{prop}={properties[prop]}");
                }
            }
            catch(ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                result = false;
                LogError(e, ErrorCategory.WriteError);
            }

            return result;
        }

        private LauncherExitCode? Run(string launcherPath, string paramFile)
        {
            string args = $" -paramfile \"{paramFile}\"";
            Console.WriteLine($"{launcherPath}{args}");
            _launcherConsole.Clear();
            try
            {
                if (!File.Exists(launcherPath))
                {
                    throw new FileNotFoundException($"The file [{launcherPath}] does not exist!");
                }
                else if (!File.Exists(paramFile))
                {
                    throw new FileNotFoundException($"The file [{paramFile}] does not exist!");
                }
                ProcessStartInfo info = new()
                {
                    UseShellExecute = false,
                    Arguments = args,
                    FileName = launcherPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using Process launcher = new() { StartInfo = info };
                using ManualResetEvent exitEvent = new(false);

                launcher.OutputDataReceived += (sender, e) =>
                {
                    if (!e.Data.IsNullOrWhiteSpace())
                    {
                        _launcherConsole.Append(e.Data);
                        Console.WriteLine(e.Data);
                    }
                };
                launcher.ErrorDataReceived += (sender, e) =>
                {
                    if (!e.Data.IsNullOrWhiteSpace())
                    {
                        Console.WriteLine($"Error: {e.Data}");
                        _errorToProcess.Enqueue(e.Data);
                    }
                };

                launcher.Exited += (sender, e) => exitEvent.Set();
                // Start process and begin reading output asynchronously
                launcher.EnableRaisingEvents = true;
                launcher.Start();

                launcher.BeginOutputReadLine();
                launcher.BeginErrorReadLine();

                // Wait for the process to exit without polling
                exitEvent.WaitOne();
                return (LauncherExitCode?)launcher.ExitCode;
            }
            catch (ThreadInterruptedException)
            {
                return LauncherExitCode.Aborted;
            }
            catch (OperationCanceledException)
            {
                return LauncherExitCode.Aborted;
            }
            catch (PipelineStoppedException)
            {
                return LauncherExitCode.Aborted;
            }
            catch (Exception e)
            {
                LogError(e, ErrorCategory.InvalidData);
                return LauncherExitCode.Failed;
            }
        }

        private void RunConverter(string converterPath, string outputfile)
        {
            try
            {
                string args = $" -j \"{outputfile}\" --aggregate";
                foreach (var reportFolder in _rptPaths)
                {
                    args += $" \"{reportFolder}\"";
                }
                Console.WriteLine($"{converterPath}{args}");

                ProcessStartInfo info = new()
                {
                    UseShellExecute = false,
                    Arguments = args,
                    FileName = converterPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using Process converter = new() { StartInfo = info };
                using ManualResetEvent exitEvent = new(false);

                converter.OutputDataReceived += (sender, e) => { if (!e.Data.IsNullOrWhiteSpace()) { Console.WriteLine(e.Data); } };
                converter.ErrorDataReceived += (sender, e) => { if (!e.Data.IsNullOrWhiteSpace()) { Console.WriteLine($"Error: {e.Data}"); } };
                converter.Exited += (sender, e) => exitEvent.Set();
                // Start process and begin reading output asynchronously
                converter.EnableRaisingEvents = true;
                converter.Start();

                converter.BeginOutputReadLine();
                converter.BeginErrorReadLine();

                exitEvent.WaitOne();
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                LogError(e, ErrorCategory.InvalidData);
            }
        }

        protected abstract string GetRetCodeFileName();

        protected virtual void CollateRetCode(string resdir, int retCode)
        {
            string fileName = GetRetCodeFileName();
            if (fileName.IsNullOrWhiteSpace())
            {
                LogError(new InvalidDataException("Method GetRetCodeFileName() did not return a value"), ErrorCategory.InvalidData);
                return;
            }
            if (!Directory.Exists(resdir))
            {
                ThrowTerminatingError(new ErrorRecord(new DirectoryNotFoundException($"The result folder {resdir} cannot be found."), nameof(CollateRetCode), ErrorCategory.ResourceUnavailable, nameof(CollateRetCode)));
                return;
            }
            string retCodeFilename = Path.Combine(resdir, fileName);
            try
            {
                using StreamWriter file = new (retCodeFilename, true);
                file.WriteLine(retCode.ToString());
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

        protected virtual string GetReportFilename()
        {
            return string.Empty;
        }

        protected virtual bool CollateResults(string xmlResults, string resdir)
        {
            if (!Directory.Exists(resdir))
            {
                ThrowTerminatingError(new ErrorRecord(new DirectoryNotFoundException($"The result folder {resdir} cannot be found."), nameof(CollateResults), ErrorCategory.ResourceUnavailable, nameof(CollateResults)));
            }

            string reportFileName = GetReportFilename();

            if (reportFileName.IsNullOrWhiteSpace())
            {
                LogError(new InvalidDataException("Collate results, empty reportFileName"), ErrorCategory.InvalidArgument);
                return false;
            }

            var links = GetRequiredLinksFromString(xmlResults);
            if (links.IsNullOrEmpty())
            {
                links = GetRequiredLinksFromString(_launcherConsole.ToString());
                if (links.IsNullOrEmpty())
                {
                    LogError(new FileNotFoundException("No report links in results file or log found"), ErrorCategory.InvalidData);
                    return false;
                }
            }

            try
            {
                string reportPath = Path.Combine(resdir, reportFileName);
                using StreamWriter file = new(reportPath, true);
                foreach (var link in links)
                {
                    file.WriteLine($"[Report {link.Item2}]({link.Item1})  ");
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                LogError(e, ErrorCategory.WriteError);
                return false;
            }
            return true;
        }

        private bool HasResults(string resultFile, out string xmlResults)
        {
            xmlResults = null;
            if (!File.Exists(resultFile))
            {
                WriteDebug("result file not found");
                return false;
            }

            //read result xml file
            xmlResults = File.ReadAllText(resultFile);

            if (xmlResults.IsNullOrWhiteSpace())
            {
                WriteDebug("Empty results file");
                return false;
            }
            try
            {
                var doc = XDocument.Parse(xmlResults);
                if (doc?.Root == null || !doc.Root.HasElements)
                {
                    WriteDebug("Invalid xml data in results file");
                    return false;
                }
                return true;
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                WriteDebug(e.Message);
                return false;
            }
        }

        private List<Tuple<string, string>> GetRequiredLinksFromString(string s)
        {
            if (s.IsNullOrWhiteSpace())
            {
                return null;
            }
            List<Tuple<string, string>> results = [];
            try
            {
                //report link example: td://Automation.AUTOMATION.mydph0271.hpswlabs.adapps.hp.com:8080/qcbin/TestLabModule-000000003649890581?EntityType=IRun&amp;EntityID=1195091
                Match match1 = Regex.Match(s, "td://.+?EntityID=([0-9]+)");
                Match match2 = Regex.Match(s, "tds://.+?EntityID=([0-9]+)");
                while (match1.Success)
                {
                    results.Add(new Tuple<string, string>(match1.Groups[0].Value, match1.Groups[1].Value));
                    match1 = match1.NextMatch();
                }

                while (match2.Success)
                {
                    results.Add(new Tuple<string, string>(match2.Groups[0].Value, match2.Groups[1].Value));
                    match2 = match2.NextMatch();
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                LogError(e, ErrorCategory.InvalidData);
            }
            return results;
        }
        protected void LogError(Exception ex, ErrorCategory categ = ErrorCategory.NotSpecified, [CallerMemberName] string methodName = "")
        {
            WriteError(new ErrorRecord(ex, $"{ex.GetType()}", categ, methodName));
        }
        protected void LogWarning(string warning)
        {
            WriteWarning(warning);
        }

        protected void TryDeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    WriteDebug($"Deleting file: {filePath}");
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    WriteWarning($"Error deleting file {filePath}: {ex.Message}");
                }
            }
        }
    }
}

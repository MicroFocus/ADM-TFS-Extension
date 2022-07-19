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
using PSModule.ParallelRunner.SDK.Entity;
using PRHelper = PSModule.ParallelRunner.SDK.Util.Helper;
namespace PSModule
{
    using H = Helper;
    public abstract class AbstractLauncherTaskCmdlet : PSCmdlet
    {
        #region - Private Constants

        private const string HpToolsLauncher_EXE = "HpToolsLauncher.exe";
        private const string HpToolsAborter_EXE = "HpToolsAborter.exe";
        private const string ReportConverter_EXE = "ReportConverter.exe";
        protected const string UFT_LAUNCHER = "UFT_LAUNCHER";
        protected const string PROPS = "props";
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
        private const string RUN_RESULTS_XML = "run_results.xml";

        #endregion

        private readonly StringBuilder _launcherConsole = new();
        private readonly ConcurrentQueue<string> outputToProcess = new();
        private readonly ConcurrentQueue<string> errorToProcess = new();

        protected bool _enableFailedTestsReport;
        protected bool _isParallelRunnerMode;
        protected List<TestRun> _parallelTestRuns = new();
        protected List<string> _rptPaths; // this field is instanciated in RunFromFileSystemTask\localTask.ps1 or ParallelRunnerTask\localTask.ps1 and passed to / filled in InvokeFSTaskCmdlet, then read in localTask.ps1
        protected MobileConfig _mobileConfig;

        protected AbstractLauncherTaskCmdlet() { }

        public abstract Dictionary<string, string> GetTaskProperties();

        private delegate void CreateSummaryReport(string rptPath, RunType runType, IList<ReportMetaData> reportList,
                                               bool uploadArtifact = false, ArtifactType artifactType = ArtifactType.None,
                                               string storageAccount = "", string container = "", string reportName = "", string archiveName = "");

        protected override void ProcessRecord()
        {
            string launcherPath, aborterPath = string.Empty, converterPath, paramFileName = string.Empty, resultsFileName;
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

                launcherPath = Path.GetFullPath(Path.Combine(ufttfsdir, HpToolsLauncher_EXE));
                aborterPath = Path.GetFullPath(Path.Combine(ufttfsdir, HpToolsAborter_EXE));
                converterPath = Path.GetFullPath(Path.Combine(ufttfsdir, ReportConverter_EXE));

                string propdir = Path.GetFullPath(Path.Combine(ufttfsdir, PROPS));

                if (!Directory.Exists(propdir))
                    Directory.CreateDirectory(propdir);
                if (!properties.ContainsKey(BUILD_NUMBER))
                {
                    LogError(new InvalidDataException("Missing buildNumber property!"), ErrorCategory.InvalidData);
                    return;
                }
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
                var exitCode = Run(launcherPath, paramFileName);
                var runType = (RunType)Enum.Parse(typeof(RunType), properties[RUN_TYPE]);
                bool hasResults = HasResults(resultsFileName, out string xmlResults);
                if (!hasResults)
                {
                    ErrorCategory categ = exitCode == LauncherExitCode.AlmNotConnected ? ErrorCategory.ConnectionError : ErrorCategory.InvalidData;
                    if (errorToProcess.TryDequeue(out string error))
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
                        var listReport = H.ReadReportFromXMLFile(resultsFileName, false, out _, _isParallelRunnerMode);
                        //create html report
                        if (runType == RunType.FileSystem && properties[UPLOAD_ARTIFACT] == YES)
                        {
                            string storageAccount = properties.GetValueOrDefault(STORAGE_ACCOUNT, string.Empty);
                            string container = properties.GetValueOrDefault(CONTAINER, string.Empty);
                            var artifactType = (ArtifactType)Enum.Parse(typeof(ArtifactType), properties[ARTIFACT_TYPE]);
                            createSummaryReportHandler(resdir, runType, listReport, true, artifactType, storageAccount, container, properties[REPORT_NAME], properties[ARCHIVE_NAME]);
                        }
                        else
                        {
                            createSummaryReportHandler(resdir, runType, listReport);
                        }
                        //get task return code
                        runStatus = H.GetRunStatus(listReport);
                        int totalTests = H.GetNumberOfTests(listReport, out IDictionary<string, int> nrOfTests);
                        if (totalTests > 0)
                        {
                            H.CreateRunSummary(runStatus, totalTests, nrOfTests, resdir);
                            if (runType == RunType.FileSystem)
                            {
                                if (listReport.Any())
                                {
                                    var rptPaths = listReport.Select(p => p.ReportPath).Where(p => !p.IsNullOrWhiteSpace());
                                    if (rptPaths.Any())
                                    {
                                        if (_isParallelRunnerMode)
                                        {
                                            foreach (var path in rptPaths)
                                            {
                                                var dirs = new DirectoryInfo(path).GetFiles(RUN_RESULTS_XML, SearchOption.AllDirectories).Select(f => f.Directory.FullName).OrderBy(d => d);
                                                if (dirs.Any())
                                                {
                                                    _rptPaths.AddRange(dirs);
                                                }
                                                else
                                                {
                                                    LogError(new FileNotFoundException($"The report file '{RUN_RESULTS_XML}' is not found in '{path}'."), ErrorCategory.ResourceUnavailable);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            rptPaths.ForEach(p => _rptPaths.Add(p));
                                        }
                                    }
                                }
                                if (_rptPaths.Any() && _enableFailedTestsReport)
                                {
                                    //run junit report converter
                                    string outputFileReport = Path.Combine(resdir, JUNIT_REPORT_XML);
                                    RunConverter(converterPath, outputFileReport);
                                    if (File.Exists(outputFileReport) && new FileInfo(outputFileReport).Length > 0 && nrOfTests[H.FAIL] > 0)
                                    {
                                        H.ReadReportFromXMLFile(outputFileReport, true, out IDictionary<string, IList<ReportMetaData>> failedSteps);
                                        H.CreateFailedStepsReport(failedSteps, resdir);
                                    }
                                }
                            }
                        }
                    }
                    CollateRetCode(resdir, (int)runStatus);
                }
            }
            catch (IOException ioe)
            {
                LogError(ioe);
            }
            catch (ThreadInterruptedException e)
            {
                LogError(e, ErrorCategory.OperationStopped);
                Run(aborterPath, paramFileName);
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
            Console.WriteLine($"{launcherPath} -paramfile {paramFile}");

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
                ProcessStartInfo info = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    Arguments = $" -paramfile \"{paramFile}\"",
                    FileName = launcherPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                Process launcher = new Process { StartInfo = info };

                launcher.OutputDataReceived += Launcher_OutputDataReceived;
                launcher.ErrorDataReceived += Launcher_ErrorDataReceived;

                launcher.Start();

                launcher.BeginOutputReadLine();
                launcher.BeginErrorReadLine();

                while (!launcher.HasExited)
                {
                    if (outputToProcess.TryDequeue(out string line))
                    {
                        _launcherConsole.Append(line);
                        WriteObject(line);
                    }
                }

                launcher.OutputDataReceived -= Launcher_OutputDataReceived;
                launcher.ErrorDataReceived -= Launcher_ErrorDataReceived;

                launcher.WaitForExit();

                return (LauncherExitCode?)launcher.ExitCode;
            }
            catch (ThreadInterruptedException)
            {
                throw;
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
                var info = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    Arguments = $" -j \"{outputfile}\" --aggregate",
                    FileName = converterPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                foreach (var reportFolder in _rptPaths)
                {
                    info.Arguments += $" \"{reportFolder}\"";
                }

                Process converter = new Process { StartInfo = info };

                converter.OutputDataReceived += Launcher_OutputDataReceived;
                converter.ErrorDataReceived += Launcher_ErrorDataReceived;

                converter.Start();

                converter.BeginOutputReadLine();
                converter.BeginErrorReadLine();

                while (!converter.HasExited)
                {
                    if (outputToProcess.TryDequeue(out string line))
                    {
                        WriteObject(line);
                    }

                    if (errorToProcess.TryDequeue(out line))
                    {
                        WriteObject(line);
                    }
                }

                converter.OutputDataReceived -= Launcher_OutputDataReceived;
                converter.ErrorDataReceived -= Launcher_ErrorDataReceived;

                converter.WaitForExit();
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

        private void Launcher_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!e.Data.IsNullOrWhiteSpace())
            {
                errorToProcess.Enqueue(e.Data);
            }
        }

        private void Launcher_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            outputToProcess.Enqueue(e.Data);
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
                using StreamWriter file = new StreamWriter(retCodeFilename, true);
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
            var results = new List<Tuple<string, string>>();
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
    }
}

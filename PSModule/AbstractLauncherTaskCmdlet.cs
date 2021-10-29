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
        private const string ENABLE_FAILED_TESTS_RPT = "enableFailedTestsReport";
        protected const string YES = "yes";
        private const string JUNIT_REPORT_XML = "junit_report.xml";

        #endregion

        private readonly StringBuilder _launcherConsole = new StringBuilder();
        private bool _hasLauncherErrors = false;
        private readonly ConcurrentQueue<string> outputToProcess = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> errorToProcess = new ConcurrentQueue<string>();

        protected AbstractLauncherTaskCmdlet() { }

        public abstract Dictionary<string, string> GetTaskProperties();

        protected override void ProcessRecord()
        {
            string launcherPath, aborterPath = string.Empty, converterPath, paramFileName = string.Empty, resultsFileName;
            try
            {
                Dictionary<string, string> properties;
                try
                {
                    properties = GetTaskProperties();
                    if (properties == null || !properties.Any())
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
                if (exitCode == LauncherExitCode.AlmNotConnected && _hasLauncherErrors) // currently it applies to ALM only
                {
                    CollateRetCode(resdir, (int)exitCode);
                }
                else
                {
                    RunStatus runStatus = RunStatus.FAILED;
                    //collect results
                    bool hasResults = CollateResults(resultsFileName, _launcherConsole.ToString(), resdir);
                    if (hasResults)
                    {
                        var listReport = H.ReadReportFromXMLFile(resultsFileName, false, out _);

                        var runType = (RunType)Enum.Parse(typeof(RunType), properties[RUN_TYPE]);
                        //create html report
                        if (runType == RunType.FileSystem && properties[UPLOAD_ARTIFACT] == YES)
                        {
                            string storageAccount = properties.GetValueOrDefault(STORAGE_ACCOUNT, string.Empty);
                            string container = properties.GetValueOrDefault(CONTAINER, string.Empty);
                            var artifactType = (ArtifactType)Enum.Parse(typeof(ArtifactType), properties[ARTIFACT_TYPE]);
                            H.CreateSummaryReport(resdir, runType, listReport, true, artifactType, storageAccount, container, properties[REPORT_NAME], properties[ARCHIVE_NAME]);
                        }
                        else
                        {
                            H.CreateSummaryReport(resdir, runType, listReport);
                        }
                        //get task return code
                        runStatus = H.GetRunStatus(listReport);
                        int totalTests = H.GetNumberOfTests(listReport, out IDictionary<string, int> nrOfTests);
                        if (totalTests > 0)
                        {
                            H.CreateRunSummary(runStatus, totalTests, nrOfTests, resdir);

                            var reportFolders = new List<string>();
                            foreach (var item in listReport)
                            {
                                if (!item.ReportPath.IsNullOrWhiteSpace())
                                    reportFolders.Add(item.ReportPath);
                            }

                            if (runType == RunType.FileSystem && reportFolders.Any() && properties[ENABLE_FAILED_TESTS_RPT] == YES)
                            {
                                //run junit report converter
                                string outputFileReport = Path.Combine(resdir, JUNIT_REPORT_XML);
                                RunConverter(converterPath, outputFileReport, reportFolders);
                                if (File.Exists(outputFileReport) && new FileInfo(outputFileReport).Length > 0 && nrOfTests[H.FAIL] > 0)
                                {
                                    H.ReadReportFromXMLFile(outputFileReport, true, out IDictionary<string, IList<ReportMetaData>> failedSteps);
                                    H.CreateFailedStepsReport(failedSteps, resdir);
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

            _hasLauncherErrors = false;
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

                    if (errorToProcess.TryDequeue(out line))
                    {
                        _hasLauncherErrors = true;
                        LogError(new Exception(line)); // this will print the error in red-color
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

        private void RunConverter(string converterPath, string outputfile, List<string> inputReportFolders)
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
                foreach (var reportFolder in inputReportFolders)
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
                LogError(new DirectoryNotFoundException(resdir), ErrorCategory.ResourceUnavailable);
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

        protected virtual bool CollateResults(string resultFile, string log, string resdir)
        {
            if (!File.Exists(resultFile))
            {
                LogError(new FileNotFoundException("result file does not exist"), ErrorCategory.ResourceUnavailable);
                File.Create(resultFile).Dispose();
            }

            string reportFileName = GetReportFilename();

            if (reportFileName.IsNullOrWhiteSpace())
            {
                LogError(new InvalidDataException("Collate results, empty reportFileName"), ErrorCategory.InvalidArgument);
                return false;
            }

            if ((resultFile.IsNullOrWhiteSpace() || !File.Exists(resultFile)) && log.IsNullOrWhiteSpace())
            {
                LogError(new FileNotFoundException($"No results file ({resultFile}) nor result log provided"), ErrorCategory.InvalidData);
                return false;
            }

            //read result xml file
            string xml = File.ReadAllText(resultFile);

            if (xml.IsNullOrWhiteSpace())
            {
                LogError(new FileNotFoundException("Empty results file"), ErrorCategory.InvalidData);
                return false;
            }
            else
            {
                try
                {
                    var doc = XDocument.Parse(xml);
                    if (doc?.Root == null || !doc.Root.HasElements)
                    {
                        LogError(new FileNotFoundException("Invalid or empty results file"), ErrorCategory.InvalidData);
                        return false;
                    }
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
            }
            var links = GetRequiredLinksFromString(xml);
            if (links.IsNullOrEmpty())
            {
                links = GetRequiredLinksFromString(log);
                if (links.IsNullOrEmpty())
                {
                    LogError(new FileNotFoundException("No report links in results file or log found"), ErrorCategory.InvalidData);
                    return false;
                }
            }

            try
            {
                string reportPath = Path.Combine(resdir, reportFileName);
                using StreamWriter file = new StreamWriter(reportPath, true);
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

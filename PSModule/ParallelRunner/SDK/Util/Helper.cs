using PSModule.ParallelRunner.SDK.Entity;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System;

namespace PSModule.ParallelRunner.SDK.Util
{
    using C = Common.Constants;
    public static class Helper
    {
        private static readonly string[] _toBeIgnoredStatuses = new string[] { "Running", "Pending" };
        private const string HTML_SCRIPT_XPATH = "/html/script";
        private const string VAR_JSON_PREFIX = "var json_";
        private const string JSON_END_SUFFIX = "}};";
        public const string FILE_NOT_FOUND = "ParallelRunner html report file not found";

        private static readonly char[] EQ = new char[] { '=' };
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new() { ContractResolver = new FieldsContractResolver() };

        public static List<TestRun> GetTestRuns(string reportPath)
        {
            if (HasParallelRunnerJsonReport(reportPath, out string fullPathFilename))
            {
                return GetTestRunsFromJson(fullPathFilename);
            }
            else
            {
                fullPathFilename = Path.Combine(reportPath, C.PARALLEL_RUN_RESULTS_HTML);
                return File.Exists(fullPathFilename) ? 
                    GetTestRunsFromHtml(fullPathFilename): 
                    throw new FileNotFoundException(FILE_NOT_FOUND, fullPathFilename);
            }
        }

        /// <summary>
        /// Check if the parallelrun_results.json exists in the specified path
        /// </summary>
        /// <param name="parallelRptResPath">test's ParallelReport Res subdir, like "D:\Work\UFTTests\MobileConfigP2\ParallelReport\Res5"</param>
        /// <returns></returns>
        public static bool HasParallelRunnerJsonReport(string parallelRptResPath)
        {
            return HasParallelRunnerJsonReport(parallelRptResPath, out _);
        }

        /// <summary>
        /// Check if the run_results.html exists in the specified path
        /// </summary>
        /// <param name="parallelRptPath">test's ParallelReport subdir, like "D:\Work\UFTTests\SuccessWait\ParallelReport\Res18\SuccessWait_[1]"</param>
        /// <returns></returns>
        public static bool HasUftHtmlReport(string parallelRptPath)
        {
            return File.Exists(Path.Combine(parallelRptPath, $@"Report\{C.RUN_RESULTS_HTML}"));
        }

        private static bool HasParallelRunnerJsonReport(string parallelRptResPath, out string fullPathFilename)
        {
            fullPathFilename = Path.Combine(parallelRptResPath, C.PARALLEL_RUN_RESULTS_JSON);
            return File.Exists(fullPathFilename);
        }

        private static List<TestRun> GetTestRunsFromJson(string fullPathFilename)
        {
            using StreamReader sr = new(fullPathFilename);
            string json = sr.ReadToEnd();
            TestRun[] tests = json.FromJson<TestRun[]>(_jsonSerializerSettings);
            return tests?.OrderBy(r => r.Path).ToList() ?? new();
        }
        private static List<TestRun> GetTestRunsFromHtml(string fullPathFilename)
        {
            Dictionary<int, TestRun> dict = new();
            using StreamReader sr = new(fullPathFilename);
            HtmlDocument doc = new();
            doc.Load(sr);
            var nodes = doc.DocumentNode.SelectNodes(HTML_SCRIPT_XPATH);
            if (nodes != null)
            {
                foreach (var node in nodes.Where(s => s.InnerText.TrimStart().StartsWith(VAR_JSON_PREFIX)))
                {
                    var str = node.InnerText.Split(EQ, 2)[1];
                    var len = str.IndexOf(JSON_END_SUFFIX) + 2;
                    var json = str.Substring(0, len);
                    TestRun obj = json.FromJson<TestRun>(_jsonSerializerSettings);
                    if (obj == null || obj.Status.In(_toBeIgnoredStatuses))
                        continue;
                    if (dict.ContainsKey(obj.Id))
                    {
                        dict[obj.Id] = obj;
                    }
                    else
                    {
                        dict.Add(obj.Id, obj);
                    }
                }
            }

            return dict.Values.OrderBy(r => r.RunResultsHtmlRelativePath).ToList();
        }
    }
}
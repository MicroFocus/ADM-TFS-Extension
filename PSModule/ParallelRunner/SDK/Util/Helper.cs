using PSModule.ParallelRunner.SDK.Entity;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace PSModule.ParallelRunner.SDK.Util
{
    public static class Helper
    {
        private static readonly string[] _toBeIgnoredStatuses = new string[] { "Running", "Pending" };
        private const string PARALLEL_RUN_RESULTS_HTML = "parallelrun_results.html";
        private const string HTML_SCRIPT_XPATH = "/html/script";
        private const string VAR_JSON_PREFIX = "var json_";
        private const string JSON_END_SUFFIX = "}};";
        public const string FILE_NOT_FOUND = "ParallelRunner html report file not found";

        private static readonly char[] EQ = new char[] { '=' };

        public static List<TestRun> GetTestRuns(string reportPath)
        {
            string fullPathFilename = Path.Combine(reportPath, PARALLEL_RUN_RESULTS_HTML);
            if (!File.Exists(fullPathFilename))
            {
                throw new FileNotFoundException(FILE_NOT_FOUND, fullPathFilename);
            }

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
                    TestRun obj = json.FromJson<TestRun>();
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

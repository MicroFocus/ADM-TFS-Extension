using PSModule;
using PSModule.AlmLabMgmtClient.Result.Model;
using PSModule.AlmLabMgmtClient.SDK.Util;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace AlmLabMgmtClient.SDK.Util
{
    public class JUnitParser
    {
        private readonly string _entityId;
        private const string TESTSET_NAME = "testset-name";
        private const string TEST_SUBTYPE = "test-subtype";
        private const string TESTCYCL_ID = "testcycl-id";
        private const string TEST_CONFIG_NAME = "test-config-name";
        private const string DURATION = "duration";
        private const string PASSED = "Passed";
        private const string ZERO = "0";

        public JUnitParser(string entityId)
        {
            _entityId = entityId;
        }

        public TestSuites ToModel(
                IList<IDictionary<string, string>> testInstanceRuns,
                string entityName,
                string url,
                string domain,
                string project)
        {
            var testSetIdToTestSuite = GetTestSets(testInstanceRuns);
            AddTestCases(
                    testInstanceRuns,
                    testSetIdToTestSuite,
                    entityName,
                    url,
                    domain,
                    project);

            return CreateTestSuites(testSetIdToTestSuite);
        }

        private TestSuites CreateTestSuites(IDictionary<string, TestSuite> testSetIdToTestsuite)
        {
            TestSuites res = new TestSuites();
            List<TestSuite> testsuites = res.ListOfTestSuites;
            foreach (TestSuite currTestsuite in testSetIdToTestsuite.Values)
            {
                testsuites.Add(currTestsuite);
            }

            return res;
        }

        private void AddTestCases(
                IList<IDictionary<string, string>> testInstanceRuns,
                IDictionary<string, TestSuite> testSetIdToTestsuite,
                string bvsName,
                string url,
                string domain,
                string project)
        {
            foreach (IDictionary<string, string> currEntity in testInstanceRuns)
            {
                AddTestCase(
                        testSetIdToTestsuite,
                        currEntity,
                        bvsName,
                        url,
                        domain,
                        project);
            }
        }

        private void AddTestCase(
                IDictionary<string, TestSuite> testSetIdToTestsuite,
                IDictionary<string, string> currEntity,
                string bvsName,
                string url,
                string domain,
                string project)
        {
            if (testSetIdToTestsuite.TryGetValue(currEntity[TESTCYCL_ID], out TestSuite testsuite))
            {
                testsuite.ListOfTestCases.Add(GetTestCase(currEntity, bvsName, url, domain, project));
            }
        }

        private TestCase GetTestCase(
                IDictionary<string, string> entity,
                string bvsName,
                string url,
                string domain,
                string project)
        {

            TestCase ret = new TestCase
            {
                Classname = GetTestSetName(entity, bvsName),
                Name = GetTestName(entity),
                Time = GetTime(entity),
                Type = entity[TEST_SUBTYPE]
            };
            TestCaseStatusUpdater.Update(ret, entity, url, domain, project);

            return ret;
        }

        private string GetTestSetName(IDictionary<string, string> entity, string bvsName)
        {
            string ret = $"{bvsName}.(Unnamed test set)";
            entity.TryGetValue(TESTSET_NAME, out string testSetName);
            if (!testSetName.IsNullOrWhiteSpace())
            {
                ret = $"{bvsName} (id:{_entityId}).{testSetName}";
            }
            return ret;
        }

        private string GetTestName(IDictionary<string, string> entity)
        {
            entity.TryGetValue(TEST_CONFIG_NAME, out string testName);
            if (testName.IsNullOrWhiteSpace())
            {
                testName = "Unnamed test";
            }

            return testName;
        }

        private string GetTime(IDictionary<string, string> entity)
        {
            entity.TryGetValue(DURATION, out string time);
            if (time.IsNullOrWhiteSpace())
            {
                time = ZERO;
            }
            else
            {
                time = $"{double.Parse(time) * 1000}";
            }

            return time;
        }

        private IDictionary<string, TestSuite> GetTestSets(IList<IDictionary<string, string>> testInstanceRuns)
        {
            var res = new Dictionary<string, TestSuite>();
            foreach (var currEntity in testInstanceRuns)
            {
                currEntity.TryGetValue(TESTCYCL_ID, out string testSetId);
                if (!res.ContainsKey(testSetId))
                {
                    res.Add(testSetId, new TestSuite());
                }
            }

            return res;
        }

        private static class TestCaseStatusUpdater
        {
            private const string STATUS = "status";
            private const string RUN_ID = "run-id";

            public static void Update(
                        TestCase testcase,
                        IDictionary<string, string> entity,
                        string url,
                        string domain,
                        string project)
            {
                entity.TryGetValue(STATUS, out string status);
                testcase.Status = GetAzureStatus(status);
                if (testcase.Status == JUnitTestCaseStatus.ERROR)
                {
                    string errorMessage = status;
                    if (errorMessage != null)
                    {
                        var link = GetTestInstanceRunLink(entity, url, domain, project);
                        testcase.ListOfErrors.Add(new Error { Message = $"Error: {errorMessage}. {link}" });
                    }
                }
            }

            private static string GetTestInstanceRunLink(
                    IDictionary<string, string> entity,
                    string url,
                    string domain,
                    string project)
            {
                string res = string.Empty;
                entity.TryGetValue(RUN_ID, out string runId);
                if (!runId.IsNullOrWhiteSpace())
                {
                    try
                    {
                        res = $"To see the test instance run in ALM, go to: td://{project}.{domain}.{new Uri(url).Host}:8080/qcbin/[TestRuns]?EntityLogicalName=run&EntityID={runId}";
                    }
                    catch (UriFormatException ex)
                    {
                        throw new AlmException($"{url}: {ex.Message}", ErrorCategory.ParserError);
                    }
                }

                return res;
            }

            private static string GetAzureStatus(string status)
            {
                return status == PASSED
                        ? JUnitTestCaseStatus.PASS
                        : JUnitTestCaseStatus.ERROR;
            }
        }
    }
}
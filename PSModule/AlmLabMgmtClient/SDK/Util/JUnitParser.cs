/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2023 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

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
        private const string START_EXEC_TIME = "start-exec-time";
        private const string START_EXEC_DATE = "start-exec-date";
        private const string PASSED = "Passed";
        private const string FAILED = "Failed";
        private const string NO_RUN = "No Run";
        private const string ZERO = "0";

        public JUnitParser(string entityId)
        {
            _entityId = entityId;
        }

        public TestSuites ToModel(IList<IDictionary<string, string>> testInstanceRuns, string entityName, string url, string domain, string project)
        {
            var testSets = BuildTestSets(testInstanceRuns, entityName, url, domain, project);

            return CreateTestSuites(testSets);
        }

        private TestSuites CreateTestSuites(IDictionary<string, TestSuite> testSets)
        {
            TestSuites res = new TestSuites();
            List<TestSuite> testsuites = res.ListOfTestSuites;
            testSets.Values.ForEach(ts => testsuites.Add(ts));
            return res;
        }

        private IDictionary<string, TestSuite> BuildTestSets(IList<IDictionary<string, string>> testInstanceRuns, string bvsName, string url, string domain, string project)
        {
            Dictionary<string, TestSuite> testSets = [];
            foreach (var entity in testInstanceRuns)
            {
                entity.TryGetValue(TESTCYCL_ID, out string testSetId);
                TestSuite testsuite;
                if (testSets.ContainsKey(testSetId))
                {
                    testsuite = testSets[testSetId];
                }
                else
                {
                    testsuite = new TestSuite();
                    testSets.Add(testSetId, testsuite);
                }
                testsuite.ListOfTestCases.Add(GetTestCase(entity, bvsName, url, domain, project));
            }
            return testSets;
        }

        private TestCase GetTestCase(IDictionary<string, string> entity, string bvsName, string url, string domain, string project)
        {
            TestCase tc = new TestCase
            {
                Classname = GetTestSetName(entity, bvsName),
                Name = GetTestName(entity),
                Time = GetTime(entity),
                StartExecDateTime = GetTimestamp(entity),
                Type = entity[TEST_SUBTYPE]
            };
            TestCaseStatusUpdater.Update(tc, entity, url, domain, project);

            return tc;
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

            return time;
        }

        private string GetTimestamp(IDictionary<string, string> entity)
        {
            entity.TryGetValue(START_EXEC_DATE, out string date);
            entity.TryGetValue(START_EXEC_TIME, out string time);

            return $"{date} {time}";
        }

        private static class TestCaseStatusUpdater
        {
            private const string STATUS = "status";
            private const string RUN_ID = "run-id";

            public static void Update(TestCase testcase, IDictionary<string, string> entity, string url, string domain, string project)
            {
                entity.TryGetValue(STATUS, out string status);
                testcase.Status = GetAzureStatus(status);
                if (testcase.Status == JUnitTestCaseStatus.ERROR)
                {
                    testcase.ListOfErrors.Add(new Error { Message = $"{status}. {GetTestInstanceRunLink(entity, url, domain, project)}" });
                }
                else if (testcase.Status == JUnitTestCaseStatus.FAIL)
                {
                    testcase.ListOfFailures.Add(new Failure { Message = $"{status}. {GetTestInstanceRunLink(entity, url, domain, project)}" });
                }
            }

            private static string GetTestInstanceRunLink(IDictionary<string, string> entity, string url, string domain, string project)
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
                return status switch
                {
                    PASSED => JUnitTestCaseStatus.PASS,
                    NO_RUN => JUnitTestCaseStatus.ERROR,
                    FAILED => JUnitTestCaseStatus.FAIL,
                    _ => status,
                };
            }
        }
    }
}
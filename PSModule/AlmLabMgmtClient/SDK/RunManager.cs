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

using PSModule.AlmLabMgmtClient.Result;
using PSModule.AlmLabMgmtClient.Result.Model;
using PSModule.AlmLabMgmtClient.SDK.Auth;
using PSModule.AlmLabMgmtClient.SDK.Factory;
using PSModule.AlmLabMgmtClient.SDK.Handler;
using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Request;
using PSModule.AlmLabMgmtClient.SDK.Util;
using PSModule.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK
{
    using C = Constants;
    public class RunManager
    {
        public event EventHandler<string> RunIdGenerated;

        private readonly RunHandler _runHandler;
        private readonly StopRunHandler _stopRunHandler;
        private readonly PollHandler _pollHandler;
        private bool _isRunning = false;
        private bool _isPolling = false;
        private bool _isLoggedIn = false;
        private readonly ILogger _logger;

        private readonly RestClient _client;
        private readonly Args _args;
        private readonly string _fullPathReportName;

        public bool IsRunning => _isRunning;
        public bool IsPolling => _isPolling;

        public RunManager(RestClient client, Args args, string fullPathReportName = null)
        {
            _client = client;
            _logger = client.Logger;
            _args = args;
            _fullPathReportName = fullPathReportName;
            if (args.RunType == C.STOP_RUN)
            {
                _stopRunHandler = new StopRunHandler(client, args.EntityId);
                _runHandler = null;
                _pollHandler = null;
            }
            else
            {
                _runHandler = new RunHandlerFactory().Create(client, args.RunType, args.EntityId);
                _pollHandler = new PollHandlerFactory().Create(client, args.RunType, args.EntityId);
                _stopRunHandler = null;
            }
        }

        private void OnRunIdGenerated(string runId)
        {
            RunIdGenerated?.Invoke(this, runId);
        }

        public bool Login4Stop()
        {
            var authHandler = AuthManager.Instance;
            _isLoggedIn = authHandler.Authenticate(_client).Result;
            return _isLoggedIn;
        }

        public async Task<TestSuites> Execute()
        {
            TestSuites res = null;
            _isRunning = true;
            var authHandler = AuthManager.Instance;
            _isLoggedIn = await authHandler.Authenticate(_client);
            if (_isLoggedIn)
            {
                if (await IsValidBvsOrTestSet() && await Start())
                {
                    _isPolling = true;
                    if (await _pollHandler.Poll())
                    {
                        var publisher = new LabPublisher(_client, _args.EntityId, _runHandler.RunId, _runHandler.NameSuffix);
                        res = await publisher.Publish(_args.ServerUrl, _args.Domain, _args.Project);
                    }
                    _isPolling = false;
                }
                await authHandler.Logout(_client);
                _isLoggedIn = false;
            }
            _isRunning = false;

            return res;
        }

        private async Task<bool> Start()
        {
            bool ok = false;
            Response res = await _runHandler.Start(
                            _args.Duration,
                            _args.EnvironmentConfigurationId);
            if (IsOK(res))
            {
                RunResponse runResponse = _runHandler.GetRunResponse(res);
                SetRunId(runResponse);
                ok = runResponse.HasSucceeded;
            }
            //_logger.Info($"{res.Data}");
            await LogReportUrl(ok);
            return ok;
        }

        private async Task LogReportUrl(bool hasSucceeded)
        {
            if (hasSucceeded)
            {
                string reportUrl = await _runHandler.ReportUrl(_args);
                await _logger.LogInfo($"{_args.RunType} run report for run id {_runHandler.RunId} is at: {reportUrl}");
                try
                {
                    using StreamWriter file = new StreamWriter(_fullPathReportName, true);
                    await file.WriteLineAsync($"[Report {_runHandler.RunId}]({reportUrl})");
                    await _logger.LogInfo($"Created the report URL file [{_fullPathReportName}].");
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    await _logger.LogError(e.Message);
                }
            }
            else
            {
                string errMsg = $"Failed to prepare timeslot for run. No entity of type {_args.RunType} with id {_args.EntityId} exists.";
                string note = "Note: You can run only functional test sets and build verification suites using this task. Check to make sure that the configured ID is valid (and that it is not a performance test ID).";
                await _logger.LogError($"{errMsg}{Environment.NewLine}{note}");
            }
        }

        private void SetRunId(RunResponse runResponse)
        {
            string runId = runResponse.RunId;
            if (runId.IsNullOrWhiteSpace())
            {
                _logger.LogError(C.NO_RUN_ID).Wait();
                throw new AlmException(C.NO_RUN_ID, ErrorCategory.InvalidResult);
            }
            else
            {
                _runHandler.SetRunId(runId);
                _pollHandler.SetRunId(runId);
                OnRunIdGenerated(runId);
            }
        }

        private bool IsOK(Response response)
        {
            bool ok = false;
            if (response.IsOK)
            {
                _logger.LogInfo($"Executing {_args.RunType} ID: {_args.EntityId} in {_args.Domain}/{_args.Project}");
                if (!_args.Description.IsNullOrWhiteSpace())
                    _logger.LogInfo($"Description: {_args.Description}");
                ok = true;
            }
            else
            {
                if (response.Error.IsNullOrWhiteSpace())
                    _logger.LogError($"Failed to execute {_args.RunType} ID: {_args.EntityId}, ALM Server URL: {_args.ServerUrl} (Response: {response.StatusCode}");
                else
                    _logger.LogError($"Failed to start {_args.RunType} ID: {_args.EntityId}, ALM Server URL: {_args.ServerUrl} (Error: {response.Error})");
            }
            return ok;
        }

        public bool Stop()
        {
            if (_stopRunHandler != null)
            {
                try
                {
                    _logger.LogInfo("Stopping run...").Wait();
                    var res = _stopRunHandler.Stop();
                    if (_isLoggedIn)
                    {
                        _logger.LogInfo("Trying to logout...").Wait();
                        AuthManager.Instance.Logout(_client).Wait();
                        _isLoggedIn = false;
                    }
                    if (res.IsOK)
                    {
                        return true;
                    }
                    _logger.LogError($"{res.StatusCode}: {res.Error}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message).Wait();
                }
                _isRunning = false;
                _isPolling = false;
            }
            return false;
        }

        private async Task<bool> HasTestInstances()
        {
            var res = await new GetTestInstancesRequest(_client, _args.EntityId).Execute();
            bool ok = res.IsOK && Xml.HasResults(res.Data);
            if (!ok)
                await _logger.LogError($"The {C.TESTSET} {_args.EntityId} is empty!");

            return ok;
        }

        private async Task<bool> IsExistingTestSet()
        {
            var res = await new GetTestSetRequest(_client, _args.EntityId).Execute();
            return res.IsOK && Xml.HasResults(res.Data);
        }

        private async Task<bool> IsExistingBvs()
        {
            var res = await new GetBvsRequest(_client, _args.EntityId).Execute();
            return res.IsOK && Xml.HasResults(res.Data);
        }
        private async Task<bool> IsValidBvsOrTestSet()
        {
            if (_args.RunType == C.BVS)
            {
                if (await IsExistingBvs())
                {
                    return await IsValidBvs();
                }
                else
                {
                    await _logger.LogError($"No {C.BUILD_VERIFICATION_SUITE} could be found by ID {_args.EntityId}.");
                }
            }
            else
            {
                if (await IsExistingTestSet())
                {
                    return await HasTestInstances();
                }
                else
                {
                    await _logger.LogError($"No {C.TESTSET} of functional type could be found by ID {_args.EntityId}.\nNote: You can run only functional test sets and build verification suites using this task. Check to make sure that the configured ID is valid (and that it is not a performance test ID).");
                }
            }
            return false;
        }

        private async Task<IList<int>> GetBvsTestSetsIds()
        {
            Response res = await new GetBvsTestSetsRequest(_client, _args.EntityId).Execute();
            IList<int> ids = [];

            if (res == null || !res.IsOK || res.Data == null)
            {
                return ids;
            }

            return Xml.GetTestSetIds(res.ToString());
        }

        private async Task<bool> IsValidBvs()
        {
            var testSetIds = await GetBvsTestSetsIds();
            bool ok = testSetIds.Any();
            if (ok)
            {
                var res = await new GetTestInstancesRequest(_client, testSetIds).Execute();
                var nonEmptyTestSetIds = Xml.GetTestSetIds(res.ToString());
                var emptyTestSetIds = nonEmptyTestSetIds.Any() ? testSetIds.Except(nonEmptyTestSetIds) : testSetIds;
                if (emptyTestSetIds.Any())
                {
                    await _logger.LogError($"The {C.BUILD_VERIFICATION_SUITE} {_args.EntityId} is invalid. The following TestSets are empty: {string.Join(C.COMMA, emptyTestSetIds)}.");
                    ok = false;
                }
            }
            else
            {
                await _logger.LogError($"The {C.BUILD_VERIFICATION_SUITE} {_args.EntityId} is empty!");
            }
            return ok;
        }
    }
}

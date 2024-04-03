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

using PSModule.AlmLabMgmtClient.SDK.Interface;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Util
{
    public class ConsoleLogger : ILogger
    {
        private const string PLEASE_WAIT = "Please wait ....";
        private readonly bool _isDebug;

        public bool IsDebug => _isDebug;

        public ConsoleLogger(bool isDebug)
        {
            _isDebug = isDebug;
        }

        public async Task LogInfo(string msg)
        {
            await Console.Out.WriteLineAsync(msg);
        }
        public async Task LogDebug(string msg)
        {
            if (_isDebug)
            {
                await Console.Out.WriteLineAsync(msg);
            }
        }

        public async Task LogError(string err, [CallerMemberName] string methodName = "")
        {
            if (_isDebug)
                await Console.Error.WriteLineAsync($"{methodName}: {err}");
            else
                await Console.Error.WriteLineAsync($"{err}");
        }

        public async Task ShowProgress()
        {
            await Console.Out.WriteLineAsync(PLEASE_WAIT);
        }
    }
}

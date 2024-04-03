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

using PSModule.AlmLabMgmtClient.SDK.Handler;
using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Util;
using PSModule.Common;
using System.Management.Automation;

namespace PSModule.AlmLabMgmtClient.SDK.Factory
{
    using C = Constants;
    public class RunHandlerFactory
    {
        public RunHandler Create(IClient client, string runType, string entityId)
        {
            return runType switch
            {
                C.BVS => new BvsRunHandler(client, entityId),
                C.TEST_SET => new TestSetRunHandler(client, entityId),
                _ => throw new AlmException("RunHandlerFactory: Run type {runType} is Not Implmented", ErrorCategory.NotImplemented),
            };
        }
    }
}
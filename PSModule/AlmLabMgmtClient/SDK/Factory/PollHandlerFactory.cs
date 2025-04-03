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
    public class PollHandlerFactory
    {
        public PollHandler Create(IClient client, string runType, string entityId)
        {
            PollHandler ret;
            if (runType.In(C.BVS, C.TEST_SET))
            {
                ret = new LabPollHandler(client, entityId);
            }
            else
            {
                throw new AlmException("PollHandlerFactory: Unrecognized run type", ErrorCategory.InvalidType);
            }

            return ret;
        }

/*        public PollHandler Create(IClient client, string runType, string entityId, int interval)
        {
            PollHandler ret;
            if (runType.In(C.BVS, C.TEST_SET))
            {
                ret = new LabPollHandler(client, entityId, interval);
            }
            else
            {
                throw new AlmException("PollHandlerFactory: Unrecognized run type", ErrorCategory.InvalidType);
            }

            return ret;
        }*/
    }
}
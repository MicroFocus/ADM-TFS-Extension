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

using Newtonsoft.Json;
using C = PSModule.Common.Constants;

namespace PSModule.UftMobile.SDK.Entity
{
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class DeviceMetrics
    {
        private bool cpu;
		private bool memory;
        private bool freeMemory;
        private bool logs;
        private bool wifiState;
        private bool thermalState;
        private bool freeDiskSpace;
        //private bool screenshot;
        //private bool wifiSignalStrength;
        //private bool totalDiskSpace;

        public DeviceMetrics(bool cpu, bool memory, bool freeMemory, bool logs, bool wifiState, bool thermalState, bool freeDiskSpace)
        {
            this.cpu = cpu;
            this.memory = memory;
            this.freeMemory = freeMemory;
            this.logs = logs;
            this.wifiState = wifiState;
            this.thermalState = thermalState;
            this.freeDiskSpace = freeDiskSpace;
        }
    }
}

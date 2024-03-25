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

using PSModule.Common;
using PSModule.UftMobile.SDK.Entity;
using System;
using System.Collections.Generic;

namespace PSModule.UftMobile.SDK.UI
{
    public class ParallelRunnerConfig : IConfig
    {
        private readonly EnvType _envType;
        private readonly List<Device> _devices;
        private readonly List<string> _browsers;

        public EnvType EnvType => _envType;
        public List<Device> Devices => _devices;
        public List<string> Browsers => _browsers;

        public ParallelRunnerConfig(string envType, List<Device> devices, List<string> browsers)
        {
            Enum.TryParse(envType, true, out _envType);
            _devices = devices ?? [];
            _browsers = browsers ?? [];
        }
    }
}

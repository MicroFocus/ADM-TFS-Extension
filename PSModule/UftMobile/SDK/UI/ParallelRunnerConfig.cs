using PSModule.UftMobile.SDK.Entity;
using System;
using System.Collections.Generic;

namespace PSModule.UftMobile.SDK.UI
{
    public class ParallelRunnerConfig
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
            _devices = devices ?? new();
            _browsers = browsers ?? new();
        }
    }
}

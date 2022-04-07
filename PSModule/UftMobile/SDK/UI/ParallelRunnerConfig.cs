using Newtonsoft.Json;
using PSModule.UftMobile.SDK.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSModule.UftMobile.SDK.UI
{
    public class ParallelRunnerConfig
    {
        private readonly char[] sep = new char[] { '\n' };

        private readonly EnvType _envType;
        private readonly IList<Device> _devices = new List<Device>();
        private readonly IList<string> _browsers;

        public EnvType EnvType => _envType;
        public IList<Device> Devices => _devices;
        public IList<string> Browsers => _browsers;

        public ParallelRunnerConfig(string envType, string strDevices, IList<string> browsers)
        {
            Enum.TryParse(envType, true, out _envType);
            _browsers = browsers;
            strDevices?.Split(sep, StringSplitOptions.RemoveEmptyEntries)?.ForEach(d => _devices.Add(JsonConvert.DeserializeObject<Device>($"{{{d}}}")));
        }
    }
}

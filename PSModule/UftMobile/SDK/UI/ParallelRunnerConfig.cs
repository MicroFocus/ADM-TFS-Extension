using Newtonsoft.Json;
using PSModule.UftMobile.SDK.Entity;
using System.Collections.Generic;

namespace PSModule.UftMobile.SDK.UI
{
    public class ParallelRunnerConfig
    {
        private readonly char[] sep = new char[] { '\n' };

        private readonly string _envType;
        private readonly IList<Device> _devices = new List<Device>();
        private readonly IList<string> _browsers;

        public string EnvType => _envType;
        public IList<Device> Devices => _devices;
        public IList<string> Browsers => _browsers;

        public ParallelRunnerConfig(string envType, string strDevices, IList<string> browsers)
        {
            _envType = envType;
            _browsers = browsers;
            string[] devices = strDevices.Split(sep);
            foreach (string d in devices)
            {
                Device device = JsonConvert.DeserializeObject<Device>($"{{{d}}}");
                _devices.Add(device);
            }

        }
    }
}

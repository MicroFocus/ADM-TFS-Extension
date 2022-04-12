using Newtonsoft.Json;
using PSModule.UftMobile.SDK.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSModule.UftMobile.SDK.UI
{
    public class ParallelRunnerConfig
    {
        private const string DOUBLE_QUOTE = @"""";
        private readonly char[] LF = new char[] { '\n' };
        private readonly char[] COMMA = new char[] { ',' };
        private readonly char[] COLON = new char[] { ':' };

        private readonly EnvType _envType;
        private readonly IList<Device> _devices = new List<Device>();
        private readonly IList<string> _browsers;

        public EnvType EnvType => _envType;
        public IList<Device> Devices => _devices;
        public IList<string> Browsers => _browsers;

        public ParallelRunnerConfig(string envType, string strDevices, IList<string> browsers)
        {
            Enum.TryParse(envType, true, out _envType);
            if (_envType == EnvType.Mobile)
            {
                ValidateDevices(strDevices);
            }
            else if (_envType == EnvType.Web)
            {
                _browsers = browsers;
            }
        }

        private void ValidateDevices(string strDevices)
        {
            var lines = strDevices.Split(LF).Where(line => !line.IsNullOrWhiteSpace());
            if (lines.Any())
            {
                foreach (var line in lines)
                {
                    string err = @$"Invalid device line -> {line}. The expected pattern is property1:""value1"", property2:""value2""... Valid property names are: DeviceID, Manufacturer, Model, OSType and OSVersion.";
                    if (IsValidDeviceLine(line))
                    {
                        Device device;
                        try
                        {
                            device = JsonConvert.DeserializeObject<Device>($"{{{line}}}") ?? new();
                        }
                        catch
                        {
                            throw new ArgumentException(err);
                        }
                        if (device.IsEmpty())
                        {
                            throw new ArgumentException(err);
                        }
                        _devices.Add(device);
                    }
                    else
                    {
                        throw new ArgumentException(err);
                    }
                }
            }
            else
            {
                throw new ArgumentException("At least one device should be provided.");
            }

        }
        private bool IsValidDeviceLine(string line)
        {
            var pairs = line.Split(COMMA);
            foreach (string pair in pairs)
            {
                var arr = pair.Split(COLON);
                if (arr.Length != 2 || !IsValidDevicePropName(arr[0].Trim()) || !IsValidDevicePropValue(arr[1].Trim()))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsValidDevicePropValue(string val)
        {
            return val.StartsWith(DOUBLE_QUOTE) && val.EndsWith(DOUBLE_QUOTE);
        }

        private bool IsValidDevicePropName(string prop)
        {
            return !prop.IsNullOrWhiteSpace() && prop.In(true, Device.PipelineAttributes);
        }

    }
}

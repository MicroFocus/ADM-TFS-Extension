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
        private static readonly char[] LF = new char[] { '\n' };
        private static readonly char[] COMMA = new char[] { ',' };
        private static readonly char[] COLON = new char[] { ':' };

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

        public static void ParseDeviceLines(string strDevices, out List<Device> devices, out List<string> invalidDeviceLines)
        {
            devices = new();
            invalidDeviceLines = new();
            var lines = strDevices.Split(LF).Where(line => !line.IsNullOrWhiteSpace());
            if (lines.Any())
            {
                foreach (var line in lines)
                {
                    if (IsValidDeviceLine(line))
                    {
                        Device device;
                        try
                        {
                            device = JsonConvert.DeserializeObject<Device>($"{{{line}}}");
                            if (device.IsEmpty())
                                invalidDeviceLines.Add(line);
                            else
                                devices.Add(device);
                        }
                        catch
                        {
                            invalidDeviceLines.Add(line);
                        }
                    }
                    else
                    {
                        invalidDeviceLines.Add(line);
                    }
                }
            }
        }
        private static bool IsValidDeviceLine(string line)
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

        private static bool IsValidDevicePropValue(string val)
        {
            return val.StartsWith(DOUBLE_QUOTE) && val.EndsWith(DOUBLE_QUOTE);
        }

        private static bool IsValidDevicePropName(string prop)
        {
            return !prop.IsNullOrWhiteSpace() && prop.In(true, Device.PipelineAttributes);
        }

    }
}

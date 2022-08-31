using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace PSModule.UftMobile.SDK.Entity
{
    using C = Common.Constants;
    public class Device
    {
        private const string ValidOsVersionFormat = @"^(\d+)(\.\d+){0,2}$";
        private const string Lower = "<";
        private const string LowerOrEqual = "<=";
        private const string Greater = ">";
        private const string GreaterOrEqual = ">=";
        private const string DEVICE_ID = "Device ID";
        private const string OS_TYPE = "OS Type";
        private const string OS_VERSION = "OS Version";

        private static readonly string[] _pipelineAttributes = new string[] { nameof(DeviceId), nameof(Model), nameof(Manufacturer), nameof(OSType), nameof(OSVersion)};

        public string DeviceId { get; set; }
        public string LogicName { get; set; }
        public string Model { get; set; }
        public string OSType { get; set; }
        public string OSVersion { get; set; }
        public string Manufacturer { get; set; }
        public string DeviceStatus { get; set; }

        public override string ToString()
        {
            return $@"DeviceID: ""{DeviceId}"", Manufacturer: ""{Manufacturer}"", Model: ""{Model}"", OSType: ""{OSType}"", OSVersion: ""{OSVersion}""";
        }
        public string ToRawString()
        {
            return $@"deviceId: {DeviceId}, manufacturer: {Manufacturer}, model: {Model}, osType: {OSType}, osVersion: {OSVersion}";
        }

        public string ToHtmlString()
        {
            const string DEVICE_PROP_FORMAT = @"<tr><td style=""white-space:nowrap"">{0}:&nbsp;</td><td style=""font-weight:bold;overflow:visible;white-space:nowrap"">{1}</td></tr>";
            StringBuilder props = new();
            void Append (string name, string value) => props.AppendFormat(DEVICE_PROP_FORMAT, name, value);
            if (!DeviceId.IsNullOrWhiteSpace())
                Append(DEVICE_ID, DeviceId);
            if (!Manufacturer.IsNullOrWhiteSpace())
                Append(nameof(Manufacturer), Manufacturer);
            if (!Model.IsNullOrWhiteSpace())
                Append(nameof(Model), Model);
            if (!OSType.IsNullOrWhiteSpace())
                Append(OS_TYPE, OSType);
            if (!OSVersion.IsNullOrWhiteSpace())
                Append(OS_VERSION, OSVersion);
            return @$"<table border=""0"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse:collapse;"">{props}</table>";
        }

        public bool IsAvailable(IQueryable<Device> devices, out string msg)
        {
            IList<string> props = new List<string>();
            if (!Manufacturer.IsNullOrWhiteSpace())
            {
                devices = devices.Where(d => d.Manufacturer.EqualsIgnoreCase(Manufacturer));
                props.Add(@$"Manufacturer: ""{Manufacturer}""");
            }
            if (!Model.IsNullOrWhiteSpace())
            {
                devices = devices.Where(d => d.Model.EqualsIgnoreCase(Model));
                props.Add(@$"Model: ""{Model}""");
            }
            if (!OSType.IsNullOrWhiteSpace())
            {
                devices = devices.Where(d => d.OSType.EqualsIgnoreCase(OSType));
                props.Add(@$"OSType: ""{OSType}""");
            }
            if (!OSVersion.IsNullOrWhiteSpace())
            {
                devices = devices.Where(GetOSVersionFilter());
                props.Add(@$"OSVersion: ""{OSVersion}""");
            }
            msg = props.Aggregate((a, b) => $"{a}, {b}");
            return devices.Any();
        }

        private Expression<Func<Device, bool>> GetOSVersionFilter()
        {
            string version = OSVersion.Trim();
            if (version.StartsWith(LowerOrEqual))
            {
                if (IsValidOsVersion(2, out double osVersion))
                    return d => double.Parse(d.OSVersion) <= osVersion;
            }
            else if (version.StartsWith(GreaterOrEqual))
            {
                if (IsValidOsVersion(2, out double osVersion))
                    return d => double.Parse(d.OSVersion) >= osVersion;
            }
            else if (version.StartsWith(Lower))
            {
                if (IsValidOsVersion(1, out double osVersion))
                    return d => double.Parse(d.OSVersion) < osVersion;
            }
            else if (version.StartsWith(Greater))
            {
                if (IsValidOsVersion(1, out double osVersion))
                    return d => double.Parse(d.OSVersion) > osVersion;
            }
            else if (IsValidOsVersion(0, out double osVersion))
            {
                return d => double.Parse(d.OSVersion) == osVersion;
            }

            throw new ArgumentException(@$"Invalid device OSVersion format ""{version}""");
        }

        private bool IsValidOsVersion(int numOfCharsToBeRemoved, out double osVersion)
        {
            osVersion = 0.0;
            if (OSVersion.IsNullOrWhiteSpace())
                return true;

            string version = OSVersion.Trim();
            if (numOfCharsToBeRemoved > 0)
            {
                version = version.Remove(0, numOfCharsToBeRemoved).TrimStart();
            }
            if (Regex.Match(version, ValidOsVersionFormat).Success)
            {
                osVersion = double.Parse(version);
                return true;
            }
            return false;
        }

        public bool HasSecondaryProperties()
        {
            return !(Manufacturer.IsNullOrWhiteSpace() && Model.IsNullOrWhiteSpace() && OSVersion.IsNullOrWhiteSpace() && OSType.IsNullOrWhiteSpace());
        }
        public bool IsEmpty()
        {
            return (DeviceId.IsNullOrWhiteSpace() && Manufacturer.IsNullOrWhiteSpace() && Model.IsNullOrWhiteSpace() && OSVersion.IsNullOrWhiteSpace() && OSType.IsNullOrWhiteSpace());
        }

        public static void ParseLines(string strDevices, out List<Device> devices, out List<string> invalidLines)
        {
            devices = new();
            invalidLines = new();
            var lines = strDevices.Split(C.LF_).Where(line => !line.IsNullOrWhiteSpace());
            if (lines.Any())
            {
                foreach (var line in lines)
                {
                    if (TryParse(line, out var device))
                    { 
                        devices.Add(device);
                    }
                    else
                    {
                        invalidLines.Add(line);
                    }
                }
            }
        }
        public static bool TryParse(string strDevice, out Device device)
        {
            device = null;
            bool ok = false;
            if (IsValidLine(strDevice))
            {
                try
                {
                    device = JsonConvert.DeserializeObject<Device>($"{{{strDevice}}}");
                    ok = !device.IsEmpty();
                }
                catch { }
            }
            return ok;
        }
        private static bool IsValidLine(string line)
        {
            var pairs = line.Split(C.COMMA_);
            foreach (string pair in pairs)
            {
                var arr = pair.Split(C.COLON_);
                if (arr.Length != 2 || !IsValidPropName(arr[0].Trim()) || !IsValidPropValue(arr[1].Trim()))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsValidPropValue(string val)
        {
            return val.StartsWith(C.DOUBLE_QUOTE) && val.EndsWith(C.DOUBLE_QUOTE);
        }

        private static bool IsValidPropName(string prop)
        {
            return !prop.IsNullOrWhiteSpace() && prop.In(true, _pipelineAttributes);
        }

        public static explicit operator CapableDeviceFilterDetails(Device dev)
        {
            return new()
            {
                DeviceName = $"{dev.Manufacturer} {dev.Model}",
                PlatformName = dev.OSType,
                PlatformVersion = dev.OSVersion
            };
        }
    }
}

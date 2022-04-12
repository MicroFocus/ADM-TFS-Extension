using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace PSModule.UftMobile.SDK.Entity
{
    public class Device
    {
        private const string ValidOsVersionFormat = @"^(\d+)(\.\d+){0,2}$";
        private const string Lower = "<";
        private const string LowerOrEqual = "<=";
        private const string Greater = ">";
        private const string GreaterOrEqual = ">=";

        private static readonly string[] _pipelineAttributes = new string[] { nameof(DeviceId), nameof(Model), nameof(Manufacturer), nameof(OSType), nameof(OSVersion)};
        public static string[] PipelineAttributes => _pipelineAttributes;

        public string DeviceId { get; set; }
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
            return $@"deviceId: {DeviceId}, manufacturerAndModel: {Manufacturer} {Model}, osType: {OSType}, osVersion: {OSVersion}";
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
    }
}

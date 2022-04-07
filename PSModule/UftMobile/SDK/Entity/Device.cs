
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSModule.UftMobile.SDK.Entity
{
    public class Device
    {
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
                props.Add($"Manufacturer={Manufacturer}");
            }
            if (!Model.IsNullOrWhiteSpace())
            {
                devices = devices.Where(d => d.Model.EqualsIgnoreCase(Model));
                props.Add($"Model={Model}");
            }
            if (!OSType.IsNullOrWhiteSpace())
            {
                devices = devices.Where(d => d.OSType.EqualsIgnoreCase(OSType));
                props.Add($"OSType={OSType}");
            }
            if (!OSVersion.IsNullOrWhiteSpace())
            {
                devices = devices.Where(d => d.OSVersion.EqualsIgnoreCase(OSVersion));
                props.Add($"OSVersion={OSVersion}");
            }
            msg = props.Aggregate((a, b) => $"{a}, {b}");
            return devices.Any();
        }
    }
}

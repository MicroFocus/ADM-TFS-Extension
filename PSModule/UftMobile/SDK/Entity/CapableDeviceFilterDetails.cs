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


namespace PSModule.UftMobile.SDK.Entity
{
    public class CapableDeviceFilterDetails
    {
        private const string ANY = "Any";

        public string Source { get; set; } = ANY;
        public string Udid { get; set; }
        public string DeviceName { get; set; }
        public string PlatformName { get; set; }
        public string PlatformVersion { get; set; }
        public string FleetType { get; set; } = ANY.ToLower();
    }
}

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

using System.Collections.Generic;
using System.Linq;
using C = PSModule.Common.Constants;

namespace PSModule.UftMobile.SDK.Entity
{
    public class CloudBrowsers
    {
        public string[] Regions { get; set; }

        public string[] OS { get; set; }

        public CloudBrowser[] Browsers { get; set; }
    }

    public class CloudBrowser
    {
        public CloudBrowser() { }

        public string Type { get; set; }

        public VerTag[] Versions { get; set; }

        public bool IsValidVersionOrTag(string val)
        {
            return Versions.Any(vt => vt.Version.EqualsIgnoreCase(val) || vt.Tag.EqualsIgnoreCase(val));
        }

        public override string ToString() => @$"""{Type}"": {string.Join<VerTag>(C.COMMA, Versions)}";

        public class VerTag
        {
            public string Version { get; set; }
            public string Tag { get; set; }

            public override string ToString() => @$"{{ Version = {Version}, Tag = ""{Tag}"" }}";
        }
    }
}

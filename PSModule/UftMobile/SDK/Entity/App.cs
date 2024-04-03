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

using Newtonsoft.Json;
using C = PSModule.Common.Constants;

namespace PSModule.UftMobile.SDK.Entity
{
    public class App
    {
        private const string ANY = "ANY";
        private const string MC_HOME = "MC.Home";
        private const string HOME = "Home";
        public string Type { get; set; } = ANY;

        public string Id { get; set; } = MC_HOME;

        public string Name { get; set; } = HOME;

        public string Version { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        public string Identifier { get; set; } = MC_HOME;

        public bool Instrumented { get; set; }

        public string AppLocalPath { get; set; }

        public string UrlScheme { get; set; }

        [JsonIgnore]
        public string Icon { get; set; }

        [JsonIgnore]
        public bool ApplicationExists { get; set; }

        [JsonIgnore]
        public bool InstrumentedApplicationExists { get; set; }

        public int Counter { get; set; }

        [JsonIgnore]
        public string AppVersion { get; set; }

        [JsonIgnore]
        public string AppBuildVersion { get; set; }

        public string Source { get; set; }

        [JsonIgnore]
        public Workspace[] Workspaces { get; set; }

        public override string ToString()
        {
            return @$"Name: ""{Name}"", Identifier: ""{Identifier}"", Version: ""{Version}"", Type: ""{Type}"", Source: ""{Source}""";
        }

        public App() { }

        [JsonIgnore]
        public string Json4JobUpdate => @$"{{""type"":""{Type}"",""identifier"":""{Identifier}"",""instrumented"":{(Instrumented ? C.TRUE : C.FALSE)}}}";
    }
}

using Newtonsoft.Json;
using PSModule.Common;

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

        public string FileName { get; set; }

        public string Identifier { get; set; } = MC_HOME;

        public bool Instrumented { get; set; }

        public string AppLocalPath { get; set; }

        [JsonIgnore]
        public string Icon { get; set; }

        [JsonIgnore]
        public bool ApplicationExists { get; set; }

        [JsonIgnore]
        public bool InstrumentedApplicationExists { get; set; }

        public int Counter { get; set; }

        public string AppVersion { get; set; }

        public string AppBuildVersion { get; set; }

        public string Source { get; set; }

        public Workspace[] Workspaces { get; set; }

        public override string ToString()
        {
            return @$"Name: ""{Name}"", Identifier: ""{Identifier}"", Version: ""{Version}"", Type: ""{Type}"", Source: ""{Source}""";
        }

        public App() { }

        /*public App(IMobileCenterApplication iApp)
        {
            type = iApp.Type;
            id = iApp.ID;
            name = iApp.Name;
            version = iApp.Version;
            fileName = iApp.FileName;
            identifier = iApp.Identifier;
            instrumented = iApp.Instrumented;
            counter = iApp.Counter;
            appVersion = iApp.AppVersion;
            appBuildVersion = iApp.AppBuildVersion;
        }*/

        public App(string app)
        {
            Instrumented = true;
            int idxComma = app.IndexOf(Constants.COMMA);
            if (idxComma >= 0)
            {
                Identifier = app.Substring(0, idxComma).Trim();
                Version = app.Substring(idxComma + 1).Trim();
            }
        }
    }
}

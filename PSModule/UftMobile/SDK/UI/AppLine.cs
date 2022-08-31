
using Newtonsoft.Json;
using PSModule.UftMobile.SDK.Entity;
using System.Collections.Generic;
using System.Linq;

namespace PSModule.UftMobile.SDK.UI
{
    using C = Common.Constants;
    public class AppLine
    {
        private static readonly string[] _pipelineAttributes = new string[] { nameof(Id), nameof(Name), nameof(Identifier), nameof(packaged) };

        [JsonProperty]
        private string packaged = string.Empty;

        //public properties are serialized by default
        public string Id { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; }

        public bool UsePackaged => packaged?.In(true, C.YES, C.TRUE) == true;

        public static bool TryParse(string line, out AppLine app)
        {
            return TryParseLine(line.Trim(), out app);
        }
        public static void TryParse(string appsLines, out List<AppLine> apps, out List<string> invalidLines)
        {
            apps = new();
            invalidLines = new();
            var lines = appsLines.Split(C.LF_).Where(line => !line.IsNullOrWhiteSpace());
            if (lines.Any())
            {
                foreach (var line in lines)
                {
                    if (TryParseLine(line, out var extraApp))
                    {
                        apps.Add(extraApp);
                    }
                    else
                    {
                        invalidLines.Add(line);
                    }
                }
            }
        }

        private static bool TryParseLine(string line, out AppLine app)
        {
            app = null;
            bool ok = false;
            if (IsValidLine(line))
            {
                try
                {
                    app = JsonConvert.DeserializeObject<AppLine>($"{{{line}}}");
                    if (!app.packaged.IsNullOrWhiteSpace() && !app.packaged.In(true, C.YES, C.NO, C.TRUE, C.FALSE))
                        ok = false;
                    else
                        ok = !app.IsEmpty();
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

        private bool IsEmpty()
        {
            return Id.IsNullOrWhiteSpace() && Name.IsNullOrWhiteSpace() && Identifier.IsNullOrWhiteSpace();
        }

        public bool IsAvailable(IQueryable<App> apps, out App app, out string msg)
        {
            msg = string.Empty;
            if (!Id.IsNullOrWhiteSpace())
            {
                apps = apps.Where(d => d.Id.EqualsIgnoreCase(Id));
                msg = @$"Id: ""{Id}""";
            }
            else if (!Identifier.IsNullOrWhiteSpace())
            {
                apps = apps.Where(d => d.Identifier.EqualsIgnoreCase(Identifier));
                msg = @$"Identifier: ""{Identifier}""";
            }
            else if (!Name.IsNullOrWhiteSpace())
            {
                apps = apps.Where(d => d.Name.EqualsIgnoreCase(Name));
                msg = @$"Name: ""{Name}""";
            }
            else
            {
                apps = new App[0].AsQueryable();
            }
            app = apps.FirstOrDefault();
            return apps.Any();
        }

    }
}

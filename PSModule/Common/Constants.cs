
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PSModule.Common
{
    public static class Constants
    {
        public const string APP_XML = "application/xml";
        public const string APP_JSON = "application/json";
        public const string APP_JSON_UTF8 = "application/json;charset=UTF-8";
        public const string REST = "rest";
        public const string TEST_SET = "TEST_SET";
        public const string TESTSET = "Test Set";
        public const string BVS = "BVS";
        public const string BUILD_VERIFICATION_SUITE = "Build Verification Suite";
        public const string NO_RUN_ID = "No Run ID";
        public const string ZERO = "0";
        public const string ONE = "1";
        public const string TWO = "2";
        public const string COMMA = ",";
        public const string SEMI_COLON = ";";
        public const string CLIENT = "client";
        public const string SECRET = "secret";
        public const string TENANT = "tenant";
        public const char EQUAL = '=';
        public const char COLON = ':';
        public const char SEMICOLON = ';';
        public const char LINE_FEED = '\n';

        public const string X_HP4MSECRET = "x-hp4msecret";
        public const string DeviceId = "deviceid";
        public const string OsType = "ostype";
        public const string OsVersion = "osversion";
        public const string ManufacturerAndModel = "manufacturerandmodel";

        public const string YES = "yes";
        public const string NO = "no";

        public const string HTTPS = "https";

        public const string DOUBLE_QUOTE = @"""";
        public const char DOUBLE_QUOTE_ = '"';
        public static readonly char[] LF_ = new char[] { '\n' };
        public static readonly char[] COMMA_ = COMMA.ToCharArray();
        public static readonly char[] COLON_ = new char[] { ':' };
        public static readonly char[] SEMI_COLON_ = SEMI_COLON.ToCharArray();

        public static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy()},
            Formatting = Formatting.Indented
        };

    }
}

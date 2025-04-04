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


namespace PSModule.Common
{
    public static class Constants
    {
        public const string APP_XML = "application/xml";
        public const string APP_JSON = "application/json";
        public const string APP_JSON_UTF8 = "application/json;charset=UTF-8";
        public const string REST = "rest";
        public const string TEST_SET = "TEST_SET";
        public const string STOP_RUN = "STOP_RUN";
        public const string TESTSET = "Test Set";
        public const string BVS = "BVS";
        public const string BUILD_VERIFICATION_SUITE = "Build Verification Suite";
        public const string NO_RUN_ID = "No Run ID";
        public const string MINUS_ONE = "-1";
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
        public const string TRUE = "true";
        public const string FALSE = "false";

        public const string HTTPS = "https";

        public const string DBL_QUOTE = @"""";
        public const char DBL_QUOTE_ = '"';
        public static readonly char[] LF_ = ['\n'];
        public static readonly char[] COMMA_ = COMMA.ToCharArray();
        public static readonly char[] COLON_ = [COLON];
        public static readonly char[] SEMI_COLON_ = SEMI_COLON.ToCharArray();
        public const char SLASH = '/';
        public const char BACK_SLASH = '\\';

        public const string MC = "MC";

        public const string RUN_RESULTS_XML = "run_results.xml";
        public const string RUN_RESULTS_HTML = "run_results.html";
        public const string PARALLEL_RUN_RESULTS_JSON = "parallelrun_results.json";
        public const string PARALLEL_RUN_RESULTS_HTML = "parallelrun_results.html";

        public const string LAUNCHER_EXITED_WITH_CODE = "The launcher tool exited with error code:";
        public const string DEFAULT_DT_PATTERN = "yyyy-MM-dd HH:mm:ss";

        public const string BROWSERS_ENDPOINT = "rest/v2/browser-lab/uftone/templates";
        public const string NO_BROWSER_FOUND = "No browser has been retrieved from the Functional Testing Lab server";
        public const string TOOL_VERSION = "toolVersion=2023";

        public const string Canceled = "Canceled";
        public const string LastTimestamp = "LastTimestamp";
        public const string AGENT_JOBSTATUS = "AGENT_JOBSTATUS";

    }
}
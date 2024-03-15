/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2023 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

using PSModule.UftMobile.SDK.UI;
using PSModule.Common;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PSModule
{
    /*
 * 	        runType=<Alm/FileSystem/LoadRunner>
	        almServerUrl=http://<server>:<port>/qcbin
	        almUserName=<user>
	        almPassword=<password>
	        almDomain=<domain>
	        almProject=<project>
	        almRunMode=<RUN_LOCAL/RUN_REMOTE/RUN_PLANNED_HOST>
	        almTimeout=<-1>/<numberOfSeconds>
	        almRunHost=<hostname>
	        TestSet<number starting at 1>=<testSet>/<AlmFolder>
	        Test<number starting at 1>=<testFolderPath>/<a Path ContainingTestFolders>/<mtbFilePath>

 */

    using C = Constants;
    public class LauncherParamsBuilder
    {
        private const string UPLOADARTIFACT = "uploadArtifact";
        private const string ARTIFACTTYPE = "artifactType";
        private const string BUILDNUMBER = "buildNumber";
        private const string CANCELRUNONFAILURE = "cancelRunOnFailure";
        private const string ENABLEFAILEDTESTSREPORT = "enableFailedTestsReport";
        private const string PARALLELRUNNERMODE = "parallelRunnerMode";
        private const string ENVTYPE = "envType";
        private const string REPORTNAME = "reportName";
        private const string ARCHIVENAME = "archiveName";
        private const string STORAGEACCOUNT = "storageAccount";
        private const string CONTAINER = "container";
        private const string RUNTYPE = "runType";
        private const string ALMSERVERURL = "almServerUrl";
        private const string ALMUSERNAME = "almUsername";
        private const string ALMPASSWORD = "almPassword";
        private const string ALMDOMAIN = "almDomain";
        private const string ALMPROJECT = "almProject";
        private const string SSOENABLED = "SSOEnabled";
        private const string ALMCLIENTID = "almClientID";
        private const string ALMAPIKEYSECRET = "almApiKeySecret";
        private const string ALMRUNMODE = "almRunMode";
        private const string ALMTIMEOUT = "almTimeout";
        private const string TIMESLOTDURATION = "timeslotDuration";
        private const string TESTSETID = "TestSetID";
        private const string RUNTESTTYPE = "RunTestType";
        private const string ALMTESTSETS = "almTestSets";
        private const string ALMRUNHOST = "almRunHost";
        private const string PERSCENARIOTIMEOUT = "PerScenarioTimeOut";
        private const string MOBILEHOSTADDRESS = "MobileHostAddress";
        private const string MOBILEUSERNAME = "MobileUserName";
        private const string MOBILEPASSWORD = "MobilePassword";
        private const string MOBILECLIENTID = "MobileClientId";
        private const string MOBILESECRETKEY = "MobileSecretKey";
        private const string MOBILEUSESSL = "MobileUseSSL";
        private const string MOBILETENANTID = "MobileTenantId";
        private const string MOBILEUSEPROXY = "MobileUseProxy";
        private const string MOBILEPROXYTYPE = "MobileProxyType";
        private const string MOBILEPROXYSETTING_ADDRESS = "MobileProxySetting_Address";
        private const string MOBILEPROXYSETTING_AUTHENTICATION = "MobileProxySetting_Authentication";
        private const string MOBILEPROXYSETTING_USERNAME = "MobileProxySetting_UserName";
        private const string MOBILEPROXYSETTING_PASSWORD = "MobileProxySetting_Password";
        private const string MOBILEINFO = "mobileinfo";
        private const string CLOUDBROWSER = "cloudBrowser";

        private readonly static string _secretkey = "EncriptionPass4Java";
        private readonly List<string> requiredParams = ["almRunHost", "almUserName", "almPassword"];
        private readonly Dictionary<string, string> properties = [];

        public Dictionary<string, string> GetProperties()
        {
            return properties;
        }

        public void SetUploadArtifact(string uploadArtifact)
        {
            SetParamValue(UPLOADARTIFACT, uploadArtifact);
        }

        public void SetArtifactType(ArtifactType artifactType)
        {
            SetParamValue(ARTIFACTTYPE, artifactType.ToString());
        }

        public void SetBuildNumber(string buildNumber)
        {
            SetParamValue(BUILDNUMBER, buildNumber);
        }

        public void SetCancelRunOnFailure(bool cancelRunOnFailure)
        {
            SetParamValue(CANCELRUNONFAILURE, cancelRunOnFailure ? C.YES : C.NO);
        }

        public void SetEnableFailedTestsReport(bool enableFailedTestsRpt)
        {
            SetParamValue(ENABLEFAILEDTESTSREPORT, enableFailedTestsRpt ? C.YES : C.NO);
        }

        public void SetParallelTestEnv(int testIdx, int envIdx, string device)
        {
            SetParamValue($"ParallelTest{testIdx}Env{envIdx}", device);
        }

        public void SetUseParallelRunner(bool useParallelRunner)
        {
            SetParamValue(PARALLELRUNNERMODE, $"{useParallelRunner}");
        }

        public void SetParallelRunnerEnvType(EnvType envType)
        {
            SetParamValue(ENVTYPE, $"{envType}");
        }

        public void SetReportName(string reportName)
        {
            SetParamValue(REPORTNAME, reportName);
        }

        public void SetArchiveName(string archiveName)
        {
            SetParamValue(ARCHIVENAME, archiveName);
        }

        public void SetStorageAccount(string storageAccount)
        {
            SetParamValue(STORAGEACCOUNT, storageAccount);
        }

        public void SetContainer(string container)
        {
            SetParamValue(CONTAINER, container);
        }

        public void SetRunType(RunType runType)
        {
            SetParamValue(RUNTYPE, runType.ToString());
        }

        public void SetAlmServerUrl(string almServerUrl)
        {
            SetParamValue(ALMSERVERURL, almServerUrl);
        }

        public void SetAlmUserName(string almUserName)
        {
            SetParamValue(ALMUSERNAME, almUserName);
        }

        public void SetAlmPassword(string almPassword)
        {
            string encAlmPass;
            try
            {
                encAlmPass = EncryptParameter(almPassword);
                SetParamValue(ALMPASSWORD, encAlmPass);
            }
            catch
            {

            }
        }

        public void SetAlmDomain(string almDomain)
        {
            SetParamValue(ALMDOMAIN, almDomain);
        }

        public void SetAlmProject(string almProject)
        {
            SetParamValue(ALMPROJECT, almProject);
        }

        public void SetSSOEnabled(bool ssoEnabled)
        {
            SetParamValue(SSOENABLED, $"{ssoEnabled}".ToLower());
        }

        public void SetClientID(string clientID)
        {
            SetParamValue(ALMCLIENTID, clientID);
        }

        public void SetApiKeySecret(string apiKeySecret)
        {
            string encAlmApiKey = EncryptParameter(apiKeySecret);
            SetParamValue(ALMAPIKEYSECRET, encAlmApiKey);
        }

        public void SetAlmRunMode(AlmRunMode almRunMode)
        {
            properties.Add(ALMRUNMODE, almRunMode != AlmRunMode.RUN_NONE ? almRunMode.ToString() : string.Empty);
        }

        public void SetAlmTimeout(string almTimeout)
        {
            string paramToSet = "-1";
            if (!string.IsNullOrEmpty(almTimeout))
            {
                paramToSet = almTimeout;
            }
            SetParamValue(ALMTIMEOUT, paramToSet);
        }

        public void SetTimeslotDuration(string timeslotDuration)
        {
           SetParamValue(TIMESLOTDURATION, timeslotDuration);
        }

        public void SetTestSet(int index, string testSet)
        {
            SetParamValue($"TestSet{index}", testSet);
        }

        public void SetTestSetID(string testID)
        {
            SetParamValue(TESTSETID, testID);
        }

        public void SetTestRunType(RunTestType testType)
        {
            properties.Add(RUNTESTTYPE, testType.ToString());
        }

        public void SetAlmTestSet(string testSets)
        {
            SetParamValue(ALMTESTSETS, testSets);
        }

        public void SetAlmRunHost(string host)
        {
            SetParamValue(ALMRUNHOST, host);
        }

        public void SetTest(int index, string test)
        {
            SetParamValue($"Test{index}", test);
        }

        public void SetPerScenarioTimeOut(string perScenarioTimeOut)
        {
            string paramToSet = "-1";
            if (!string.IsNullOrEmpty(perScenarioTimeOut))
            {
                paramToSet = perScenarioTimeOut;
            }
            SetParamValue(PERSCENARIOTIMEOUT, paramToSet);
        }

        public void SetDigitalLabSrvConfig(ServerConfigEx config)
        {
            if (config == null) return;
            SetParamValue(MOBILEHOSTADDRESS, config.ServerUrl);
            if (config.AuthType == UftMobile.SDK.Enums.AuthType.Basic)
            {
                SetParamValue(MOBILEUSERNAME, config.UsernameOrClientId);
                SetParamValue(MOBILEPASSWORD, EncryptParameter(config.PasswordOrSecret));
            }
            else
            {
                SetParamValue(MOBILECLIENTID, config.UsernameOrClientId);
                SetParamValue(MOBILESECRETKEY, EncryptParameter(config.PasswordOrSecret));
            }
            if (config.ServerUrl.StartsWith(C.HTTPS, StringComparison.OrdinalIgnoreCase))
            {
                SetParamValue(MOBILEUSESSL, C.ONE);
            }
            SetParamValue(MOBILETENANTID, config.TenantId != 0 ? $"{config.TenantId}" : string.Empty);
            if (config.UseProxy)
            {
                var proxy = config.ProxyConfig;
                SetParamValue(MOBILEUSEPROXY, C.ONE);
                SetParamValue(MOBILEPROXYTYPE, C.TWO);
                SetParamValue(MOBILEPROXYSETTING_ADDRESS, proxy.ServerUrl);
                if (proxy.UseCredentials)
                {
                    SetParamValue(MOBILEPROXYSETTING_AUTHENTICATION, C.ONE);
                    SetParamValue(MOBILEPROXYSETTING_USERNAME, proxy.UsernameOrClientId);
                    SetParamValue(MOBILEPROXYSETTING_PASSWORD, EncryptParameter(proxy.PasswordOrSecret));
                }
            }
        }
        public void SetMobileConfig(DeviceConfig config)
        {
            if (config == null) return;
            SetParamValue(MOBILEINFO, config.MobileInfo);
        }

        public void SetCloudBrowserConfig(CloudBrowserConfig config)
        {
            if (config == null) return;
            string cb = $@"""url={config.Url};os={config.OS};type={config.Browser};version={config.Version};region={config.Region}""";
            SetParamValue(CLOUDBROWSER, cb);
        }

        private void SetParamValue(string paramName, string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue))
            {
                if (!requiredParams.Contains(paramName))
                    properties.Remove(paramName);
                else
                    properties.Add(paramName, string.Empty);
            }
            else
            {
                properties.Add(paramName, paramValue);
            }
        }

        private string EncryptParameter(string parameter)
        {
            RijndaelManaged rijndaelCipher = new()
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,

                KeySize = 0x80,
                BlockSize = 0x80
            };
            byte[] pwdBytes = Encoding.UTF8.GetBytes(_secretkey);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(parameter);
            return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }
    }
}

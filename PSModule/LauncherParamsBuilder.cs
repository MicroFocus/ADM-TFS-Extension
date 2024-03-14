﻿/*
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
        private const string secretkey = "EncriptionPass4Java";
        private readonly List<string> requiredParameters = new List<string> { "almRunHost", "almUserName", "almPassword" };
        private readonly Dictionary<string, string> properties = new Dictionary<string, string>();

        public Dictionary<string, string> GetProperties()
        {
            return properties;
        }

        public void SetUploadArtifact(string uploadArtifact)
        {
            SetParamValue("uploadArtifact", uploadArtifact);
        }

        public void SetArtifactType(ArtifactType artifactType)
        {
            SetParamValue("artifactType", artifactType.ToString());
        }

        public void SetBuildNumber(string buildNumber)
        {
            SetParamValue("buildNumber", buildNumber);
        }

        public void SetCancelRunOnFailure(bool cancelRunOnFailure)
        {
            SetParamValue("cancelRunOnFailure", cancelRunOnFailure ? C.YES : C.NO);
        }

        public void SetEnableFailedTestsReport(bool enableFailedTestsRpt)
        {
            SetParamValue("enableFailedTestsReport", enableFailedTestsRpt ? C.YES : C.NO);
        }

        public void SetParallelTestEnv(int testIdx, int envIdx, string device)
        {
            SetParamValue($"ParallelTest{testIdx}Env{envIdx}", device);
        }

        public void SetUseParallelRunner(bool useParallelRunner)
        {
            SetParamValue("parallelRunnerMode", $"{useParallelRunner}");
        }

        public void SetParallelRunnerEnvType(EnvType envType)
        {
            SetParamValue("envType", $"{envType}");
        }

        public void SetReportName(string reportName)
        {
            SetParamValue("reportName", reportName);
        }

        public void SetArchiveName(string archiveName)
        {
            SetParamValue("archiveName", archiveName);
        }

        public void SetStorageAccount(string storageAccount)
        {
            SetParamValue("storageAccount", storageAccount);
        }

        public void SetContainer(string container)
        {
            SetParamValue("container", container);
        }

        public void SetRunType(RunType runType)
        {
            SetParamValue("runType", runType.ToString());
        }

        public void SetAlmServerUrl(string almServerUrl)
        {
            SetParamValue("almServerUrl", almServerUrl);
        }

        public void SetAlmUserName(string almUserName)
        {
            SetParamValue("almUsername", almUserName);
        }

        public void SetAlmPassword(string almPassword)
        {
            string encAlmPass;
            try
            {
                encAlmPass = EncryptParameter(almPassword);
                SetParamValue("almPassword", encAlmPass);
            }
            catch
            {

            }
        }

        public void SetAlmDomain(string almDomain)
        {
            SetParamValue("almDomain", almDomain);
        }

        public void SetAlmProject(string almProject)
        {
            SetParamValue("almProject", almProject);
        }

        public void SetSSOEnabled(bool ssoEnabled)
        {
            SetParamValue("SSOEnabled", $"{ssoEnabled}".ToLower());
        }

        public void SetClientID(string clientID)
        {
            SetParamValue("almClientID", clientID);
        }

        public void SetApiKeySecret(string apiKeySecret)
        {
            string encAlmApiKey = EncryptParameter(apiKeySecret);
            SetParamValue("almApiKeySecret", encAlmApiKey);
        }

        public void SetAlmRunMode(AlmRunMode almRunMode)
        {
            properties.Add("almRunMode", almRunMode != AlmRunMode.RUN_NONE ? almRunMode.ToString() : string.Empty);
        }

        public void SetAlmTimeout(string almTimeout)
        {
            string paramToSet = "-1";
            if (!string.IsNullOrEmpty(almTimeout))
            {
                paramToSet = almTimeout;
            }
            SetParamValue("almTimeout", paramToSet);
        }

        public void SetTimeslotDuration(string timeslotDuration)
        {
           SetParamValue("timeslotDuration", timeslotDuration);
        }

        public void SetTestSet(int index, string testSet)
        {
            SetParamValue("TestSet" + index, testSet);
        }

        public void SetTestSetID(string testID)
        {
            SetParamValue("TestSetID", testID);
        }

        public void SetTestRunType(RunTestType testType)
        {
            properties.Add("RunTestType", testType.ToString());
        }

        public void SetAlmTestSet(string testSets)
        {
            SetParamValue("almTestSets", testSets);
        }

        public void SetAlmRunHost(string host)
        {
            SetParamValue("almRunHost", host);
        }

        public void SetTest(int index, string test)
        {
            SetParamValue("Test" + index, test);
        }

        public void SetPerScenarioTimeOut(string perScenarioTimeOut)
        {
            string paramToSet = "-1";
            if (!string.IsNullOrEmpty(perScenarioTimeOut))
            {
                paramToSet = perScenarioTimeOut;
            }
            SetParamValue("PerScenarioTimeOut", paramToSet);
        }

        public void SetDigitalLabSrvConfig(ServerConfigEx config)
        {
            if (config == null) return;
            SetParamValue("MobileHostAddress", config.ServerUrl);
            if (config.AuthType == UftMobile.SDK.Enums.AuthType.Basic)
            {
                SetParamValue("MobileUserName", config.UsernameOrClientId);
                SetParamValue("MobilePassword", EncryptParameter(config.PasswordOrSecret));
            }
            else
            {
                SetParamValue("MobileClientId", config.UsernameOrClientId);
                SetParamValue("MobileSecretKey", EncryptParameter(config.PasswordOrSecret));
            }
            if (config.ServerUrl.StartsWith(C.HTTPS, StringComparison.OrdinalIgnoreCase))
            {
                SetParamValue("MobileUseSSL", C.ONE);
            }
            SetParamValue("MobileTenantId", config.TenantId != 0 ? $"{config.TenantId}" : string.Empty);
            if (config.UseProxy)
            {
                var proxy = config.ProxyConfig;
                SetParamValue("MobileUseProxy", C.ONE);
                SetParamValue("MobileProxyType", C.TWO);
                SetParamValue("MobileProxySetting_Address", proxy.ServerUrl);
                if (proxy.UseCredentials)
                {
                    SetParamValue("MobileProxySetting_Authentication", C.ONE);
                    SetParamValue("MobileProxySetting_UserName", proxy.UsernameOrClientId);
                    SetParamValue("MobileProxySetting_Password", EncryptParameter(proxy.PasswordOrSecret));
                }
            }
        }
        public void SetMobileConfig(DeviceConfig config)
        {
            if (config == null) return;
            SetParamValue("mobileinfo", config.MobileInfo);
        }

        public void SetCloudBrowserConfig(CloudBrowserConfig config)
        {
            if (config == null) return;
            string cb = $@"""url={config.Url};os={config.OS};type={config.Browser};version={config.Version};region={config.Region}""";
            SetParamValue("cloudBrowser", cb);
        }

        private void SetParamValue(string paramName, string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue))
            {
                if (!requiredParameters.Contains(paramName))
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
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 0x80;
            rijndaelCipher.BlockSize = 0x80;
            byte[] pwdBytes = Encoding.UTF8.GetBytes(secretkey);
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

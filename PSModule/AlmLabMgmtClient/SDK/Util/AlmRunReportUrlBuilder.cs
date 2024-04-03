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

using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Request;
using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Util
{
    public class AlmRunReportUrlBuilder
    {
        public async Task<string> Build(IClient client, string domain, string project, string runId)
        {
            string url = "NA";
            try
            {
                AlmVersion version = await GetAlmVersion(client);
                int majorVersion = int.Parse(version.MajorVersion);
                int minorVersion = int.Parse(version.MinorVersion);
                if (majorVersion < 12 || (majorVersion == 12 && minorVersion < 2))
                    url = client.BuildWebUIEndpoint($"lab/index.jsp?processRunId={runId}");
                else if (majorVersion >= 16) // Url change due to angular js upgrade from ALM16
                    url = client.ServerUrl.AppendSuffix($"ui/?redirected&p={domain}/{project}&execution-report#!/test-set-report/{runId}");
                else
                    url = client.ServerUrl.AppendSuffix($"ui/?redirected&p={domain}/{project}&execution-report#/test-set-report/{runId}");
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                // result url will be NA (in case of failure like getting ALM version, convert ALM version to number)
                Console.WriteLine(e.Message);
            }
            return url;
        }

        public async Task<bool> IsNewReport(IClient client)
        {
            AlmVersion version = await GetAlmVersion(client);
            // Newer than 12.2x, including 12.5x, 15.x and later
            _ = int.TryParse(version.MajorVersion, out int majorVersion);
            _ = int.TryParse(version.MinorVersion, out int minorVersion);
            return (majorVersion == 12 && minorVersion >= 2) || majorVersion > 12;
        }

        private async Task<AlmVersion> GetAlmVersion(IClient client)
        {
            Response res = await new GetAlmVersionRequest(client).Execute();
            if (res.IsOK)
                return res.Data.DeserializeXML<AlmVersion>();
            else
                throw new AlmException($"Failed to get ALM version. HTTP status code: {res.StatusCode}", ErrorCategory.InvalidResult);
        }
    }
}
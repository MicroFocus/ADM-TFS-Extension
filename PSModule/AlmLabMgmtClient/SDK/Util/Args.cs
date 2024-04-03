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

namespace PSModule.AlmLabMgmtClient.SDK.Util
{
    public class Args
    {
        public Credentials Credentials { get; internal set; }
        public string ServerUrl { get; internal set; }
        public string ClientType { get; internal set; }
        public string RunType { get; internal set; }
        public string EntityId { get; internal set; }
        public string Domain { get; internal set; }
        public string Project { get; internal set; }
        public string Duration { get; internal set; }
        public string EnvironmentConfigurationId { get; internal set; }
        public string Description { get; internal set; }
    }
}
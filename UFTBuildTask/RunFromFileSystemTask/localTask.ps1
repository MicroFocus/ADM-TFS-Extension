# 
# MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
# 
# Copyright 2016-2023 Open Text
# 
# The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
# Nothing herein should be construed as constituting an additional warranty.
# Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
# The information contained herein is subject to change without notice.
# 

using namespace PSModule.UftMobile.SDK.UI
using namespace PSModule.UftMobile.SDK.Entity
using namespace System.Collections.Generic

param()
$testPathInput = Get-VstsInput -Name 'testPathInput' -Require
$timeOutIn = Get-VstsInput -Name 'timeOutIn'
$uploadArtifact = Get-VstsInput -Name 'uploadArtifact' -Require
$artifactType = Get-VstsInput -Name 'artifactType'
$rptFileName = Get-VstsInput -Name 'reportFileName'
[string]$tsPattern = Get-VstsInput -Name 'tsPattern'
[bool]$cancelRunOnFailure = Get-VstsInput -Name 'cancelRunOnFailure' -AsBool
[bool]$enableFailedTestsRpt = Get-VstsInput -Name 'enableFailedTestsReport' -AsBool

$mcServerUrl = Get-VstsInput -Name 'mcServerUrl'

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll
$configs = [List[IConfig]]::new()

# $env:SYSTEM can be used also to determine the pipeline type "build" or "release"
if ($env:SYSTEM_HOSTTYPE -eq "build") {
	$buildNumber = $env:BUILD_BUILDNUMBER
	[int]$rerunIdx = [convert]::ToInt32($env:SYSTEM_STAGEATTEMPT, 10) - 1
	$rerunType = "rerun"
	$workDir = $env:PIPELINE_WORKSPACE
} else {
	$buildNumber = $env:RELEASE_RELEASEID
	[int]$rerunIdx = $env:RELEASE_ATTEMPTNUMBER
	$rerunType = "attempt"
}

if (![string]::IsNullOrWhiteSpace($mcServerUrl)) {
	$mcAuthType = Get-VstsInput -Name 'mcAuthType' -Require
	$mcUsername = Get-VstsInput -Name 'mcUsername'
	$mcPassword = Get-VstsInput -Name 'mcPassword'
	$mcAccessKey = Get-VstsInput -Name 'mcAccessKey'
	[bool]$useMcProxy = Get-VstsInput -Name 'useMcProxy' -AsBool
	[ProxyConfig]$proxyConfig = $null
	[bool]$isBasicAuth = ($mcAuthType -eq "basic")

	if ($isBasicAuth -and [string]::IsNullOrWhiteSpace($mcUsername)) {
		throw "Digital Lab Username is empty."
	} elseif (!$isBasicAuth -and [string]::IsNullOrWhiteSpace($mcAccessKey)) {
		throw "Digital Lab AccessKey is empty."
	} 
	if ($useMcProxy) {
		$mcProxyUrl = Get-VstsInput -Name 'mcProxyUrl'
		[bool]$useMcProxyCredentials = Get-VstsInput -Name 'useMcProxyCredentials' -AsBool
		$mcProxyUsername = Get-VstsInput -Name 'mcProxyUsername'
		$mcProxyPassword = Get-VstsInput -Name 'mcProxyPassword'

		if ([string]::IsNullOrWhiteSpace($mcProxyUrl)) {
			throw "Proxy Server is empty."
		} elseif ($useMcProxyCredentials -and [string]::IsNullOrWhiteSpace($mcProxyUsername)) {
			throw "Proxy Username is empty."
		}
		$proxySrvConfig = [ServerConfig]::new($mcProxyUrl, $mcProxyUsername, $mcProxyPassword)
		$proxyConfig = [ProxyConfig]::new($proxySrvConfig, $useMcProxyCredentials)
	}
	if ($isBasicAuth) {
		$srvConfig = [ServerConfig]::new($mcServerUrl, $mcUsername, $mcPassword)
	} else {
		$mcClientId = $mcSecret = $mcTenantId = $null
		$err = [ServerConfig]::ParseAccessKey($mcAccessKey, [ref]$mcClientId, [ref]$mcSecret, [ref]$mcTenantId)
		if ($err) {
			throw $err
		}
		$srvConfig = [ServerConfig]::new($mcServerUrl, $mcClientId, $mcSecret, $mcTenantId, $false)
	}
	$dlServerConfig = [ServerConfigEx]::new($srvConfig, $useMcProxy, $proxyConfig)
	$configs.Add($dlServerConfig)

	[bool]$useMcDevice = Get-VstsInput -Name 'useMcDevice' -AsBool
	[bool]$useCloudBrowser = Get-VstsInput -Name 'useCloudBrowser' -AsBool

	if ($useMcDevice) {
		[Device]$device = $null
		[AppLine]$app = $null
		[List[AppLine]]$apps = $null
		[List[string]]$invalidAppLines = $null
		[DeviceMetrics]$metrics = $null
		[bool]$mcInstall = $false
		[bool]$mcUninstall = $false
		[bool]$mcRestart = Get-VstsInput -Name 'mcRestart' -AsBool

		$mcDevice = (Get-VstsInput -Name 'mcDevice').Trim()
		$mcAppType = Get-VstsInput -Name 'mcAppType'
		$mcSysApp = $null
		$mcApp = (Get-VstsInput -Name 'mcApp').Trim()
		$mcExtraApps = (Get-VstsInput -Name 'mcExtraApps').Trim()

		if ($mcDevice -eq "") {
			throw "The Device field is required."
		} elseif ($false -eq [Device]::TryParse($mcDevice, [ref]$device)) {
			throw "Invalid device -> $($line). The expected pattern is property1:""value1"", property2:""value2""... Valid property names are: DeviceID, Manufacturer, Model, OSType and OSVersion.";
		} elseif ($mcAppType -eq "custom") {
			[bool]$isOK = [AppLine]::TryParse($mcApp, [ref]$app)
			if (!$isOK) {
				throw "The Main Digital Lab Application is invalid. The expected pattern is Identifier:""value"", Packaged:""Yes/No"" (Identifier is required, Packaged is optional)."
			}
		} elseif ($mcAppType -eq "system") {
			$mcSysApp = Get-VstsInput -Name 'mcSysApp'
		}
		[AppLine]::TryParse($mcExtraApps, [ref]$apps, [ref]$invalidAppLines)
		if ($invalidAppLines -and $invalidAppLines.Count -gt 0) {
			foreach ($line in $invalidAppLines) {
				Write-Warning "Invalid app line -> $($line). The expected pattern is Identifier:""value"", Packaged:""Yes/No"" (Identifier is required, Packaged is optional).";
			}
		}
		if ($app -or ($apps -and $apps.Count -gt 0))
		{
			$mcInstall = Get-VstsInput -Name 'mcInstall' -AsBool
			$mcUninstall = Get-VstsInput -Name 'mcUninstall' -AsBool
		}

		[bool]$mcLogDeviceMetrics = Get-VstsInput -Name 'mcLogDeviceMetrics' -AsBool
		if ($mcLogDeviceMetrics) {
			[bool]$mcCPU = Get-VstsInput -Name 'mcCPU' -AsBool
			[bool]$mcMemory = Get-VstsInput -Name 'mcMemory' -AsBool
			[bool]$mcFreeMemory = Get-VstsInput -Name 'mcFreeMemory' -AsBool
			[bool]$mcLogs = Get-VstsInput -Name 'mcLogs' -AsBool
			[bool]$mcWifiState = Get-VstsInput -Name 'mcWifiState' -AsBool
			[bool]$mcThermalState = Get-VstsInput -Name 'mcThermalState' -AsBool
			[bool]$mcFreeDiskSpace = Get-VstsInput -Name 'mcFreeDiskSpace' -AsBool
			$metrics = [DeviceMetrics]::new($mcCPU, $mcMemory, $mcFreeMemory, $mcLogs, $mcWifiState, $mcThermalState, $mcFreeDiskSpace)
		}

		$appAction = [AppAction]::new($mcInstall, $mcUninstall, $mcRestart)
		$appConfig = [AppConfig]::new($mcAppType, $mcSysApp, $app, $apps, $metrics, $appAction)
		$deviceConfig = [DeviceConfig]::new($device, $appConfig, $workDir)
		$configs.Add($deviceConfig)
	}
	if ($useCloudBrowser) {
		$url = (Get-VstsInput -Name 'cbStartUrl').Trim(' "')
		$region = (Get-VstsInput -Name 'cbLocation').Trim(' "')
		$os = (Get-VstsInput -Name 'cbOS').Trim(' "')
		$browser = (Get-VstsInput -Name 'cbName').Trim(' "')
		$version = (Get-VstsInput -Name 'cbVersion').Trim(' "')
		if ($region -eq "") {
			throw "The Cloud Browser Location field is required."
		} elseif ($os -eq "") {
			throw "The Cloud Browser OS field is required."
		} elseif ($browser -eq "") {
			throw "The Cloud Browser field is required."
		} elseif ($version -eq "") {
			throw "The Cloud Browser Version field is required."
		}
		$cbConfig = [CloudBrowserConfig]::new($url, $region, $os, $browser, $version)
		$configs.Add($cbConfig)
	} 
}

$resDir = Join-Path $uftworkdir -ChildPath "res\Report_$buildNumber"

#---------------------------------------------------------------------------------------------------

function UploadArtifactToAzureStorage($storageContext, $container, $testPathReportInput, $artifact) {
	#upload artifact to storage container
	Set-AzStorageBlobContent -Container "$($container)" -File $testPathReportInput -Blob $artifact -Context $storageContext
}

function ArchiveReport($artifact, $rptFolder) {
	if (Test-Path $rptFolder) {
		$fullPathZipFile = Join-Path $rptFolder -ChildPath $artifact
		Compress-Archive -Path $rptFolder -DestinationPath $fullPathZipFile
		return $fullPathZipFile
	}
	return $null
}

function UploadHtmlReport() {
	$index = 0
	foreach ( $item in $rptFolders ) {
		$testPathReportInput = Join-Path $item -ChildPath "run_results.html"
		if (Test-Path -LiteralPath $testPathReportInput) {
			$artifact = $rptFileNames[$index]
			# upload resource to container
			UploadArtifactToAzureStorage $storageContext $container $testPathReportInput $artifact
		}
		$index += 1
	}
}

function UploadArchive() {
	$index = 0
	foreach ( $item in $rptFolders ) {
		#archive report folder	
		$artifact = $zipFileNames[$index]
		
		$fullPathZipFile = ArchiveReport $artifact $item
		if ($fullPathZipFile) {
			UploadArtifactToAzureStorage $storageContext $container $fullPathZipFile $artifact
		}
					
		$index += 1
	}
}

#---------------------------------------------------------------------------------------------------

$uftReport = "$resDir\UFT Report"
$runSummary = "$resDir\Run Summary"
$retcodefile = "$resDir\TestRunReturnCode.txt"
$failedTests = "$resDir\Failed Tests"

$rptFolders = [List[string]]::new()
$rptFileNames = [List[string]]::new()
$zipFileNames = [List[string]]::new()

if ($rptFileName) {
	$rptFileName += "_$buildNumber"
} else {
	$rptFileName = "${pipelineName}_${buildNumber}"
}
if ($rerunIdx) {
	$rptFileName += "_$rerunType$rerunIdx"
}

$archiveNamePattern = "${rptFileName}_Report"

#---------------------------------------------------------------------------------------------------
#storage variables validation

if($uploadArtifact -eq "yes") {
	# get resource group
	if ($null -eq $env:RESOURCE_GROUP) {
		Write-Error "Missing resource group."
	} else {
		$group = $env:RESOURCE_GROUP
		$resourceGroup = Get-AzResourceGroup -Name "$($group)"
		$groupName = $resourceGroup.ResourceGroupName
	}

	# get storage account
	$account = $env:STORAGE_ACCOUNT

	$storageAccounts = Get-AzStorageAccount -ResourceGroupName "$($groupName)"

	$correctAccount = 0
	foreach($item in $storageAccounts) {
		if ($item.storageaccountname -like $account) {
			$storageAccount = $item
			$correctAccount = 1
			break
		}
	}

	if ($correctAccount -eq 0) {
		if ([string]::IsNullOrEmpty($account)) {
			Write-Error "Missing storage account."
		} else {
			Write-Error ("Provided storage account {0} not found." -f $account)
		}
	} else {
		$storageContext = $storageAccount.Context
		
		#get container
		$container = $env:CONTAINER

		$storageContainer = Get-AzStorageContainer -Context $storageContext -ErrorAction Stop | where-object {$_.Name -eq $container}
		if ($storageContainer -eq $null) {
			if ([string]::IsNullOrEmpty($container)) {
				Write-Error "Missing storage container."
			} else {
				Write-Error ("Provided storage container {0} not found." -f $container)
			}
		}
	}
}

if ($rerunIdx) {
	Write-Host "$((Get-Culture).TextInfo.ToTitleCase($rerunType)) = $rerunIdx"
	if (Test-Path $runSummary) {
		try {
			Remove-Item $runSummary -ErrorAction Stop
		} catch {
			Write-Error "Cannot rerun because the file '$runSummary' is currently in use."
		}
	}
	if (Test-Path $uftReport) {
		try {
			Remove-Item $uftReport -ErrorAction Stop
		} catch {
			Write-Error "Cannot rerun because the file '$uftReport' is currently in use."
		}
	}
	if (Test-Path $failedTests) {
		try {
			Remove-Item $failedTests -ErrorAction Stop
		} catch {
			Write-Error "Cannot rerun because the file '$failedTests' is currently in use."
		}
	}
}

# validate $tsPattern
try {
	[DateTime]$dtNow = Get-Date
	$dtNow.ToString($tsPattern.Trim())
} catch {
	Write-Error "Invalid Timestamp Pattern '$tsPattern'"
}
#---------------------------------------------------------------------------------------------------
#Run the tests
try {
	Invoke-FSTask $testPathInput $timeOutIn $uploadArtifact $artifactType $env:STORAGE_ACCOUNT $env:CONTAINER $rptFileName $archiveNamePattern $buildNumber $enableFailedTestsRpt $false $configs $rptFolders $cancelRunOnFailure $tsPattern -Verbose 
} catch {
	Write-Error $_
} finally {
	$ind = 1
	foreach ($item in $rptFolders) {
		$rptFileNames.Add("${rptFileName}_${ind}.html")
		$zipFileNames.Add("${rptFileName}_Report_${ind}.zip")
		$ind += 1
	}

	#---------------------------------------------------------------------------------------------------
	#upload artifacts to Azure storage
	if (($uploadArtifact -eq "yes") -and ($rptFolders.Count -gt 0)) {
		if ($artifactType -eq "onlyReport") { #upload only report
			UploadHtmlReport
		} elseif ($artifactType -eq "onlyArchive") { #upload only archive
			UploadArchive
		} else { #upload both report and archive
			UploadHtmlReport
			UploadArchive
		}
	}

	#---------------------------------------------------------------------------------------------------
	# uploads report files to build artifacts
	$all = "$resDir\all_" + $rerunIdx
	if ((Test-Path $runSummary) -and (Test-Path $uftReport)) {
		$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'
		$html = [System.Text.StringBuilder]""
		$html.Append("<div class=`"margin-right-8 margin-left-8 padding-8 depth-8`"><div class=`"body-xl`">Run Sumary</div>")
		$html.AppendLine((Get-Content -Path $runSummary))
		$html.AppendLine("</div><div class=`"margin-8 padding-8 depth-8`"><div class=`"body-xl`">UFT Report</div>")
		$html.AppendLine((Get-Content -Path $uftReport))
		$html.AppendLine("</div>")
		if (Test-Path $failedTests) {
			$html.AppendLine("<div class=`"margin-8 padding-8 depth-8`"><div class=`"body-xl`">Failed Tests</div>")
			$html.AppendLine((Get-Content -Path $failedTests))
			$html.AppendLine("</div>")
		}
		$html.ToString() >> $all
		if ($rerunIdx) {
			Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=Reports ($rerunType $rerunIdx);]$all"
		} else {
			Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=Reports;]$all"
		}
	}

	# read return code
	if (Test-Path $retcodefile) {
		$content = Get-Content $retcodefile
		if ($content) {
			$sep = [Environment]::NewLine
			$option = [System.StringSplitOptions]::RemoveEmptyEntries
			$arr = $content.Split($sep, $option)
			[int]$retcode = [convert]::ToInt32($arr[-1], 10)
	
			if ($retcode -eq 0) {
				Write-Host "Test passed"
			}

			if ($retcode -eq -3) {
				Write-Error "Task Failed with message: Closed by user"
			} elseif ($retcode -ne 0) {
				Write-Error "Task Failed"
			}
		} else {
			Write-Error "The file [$retcodefile] is empty!"
		}
	}
}
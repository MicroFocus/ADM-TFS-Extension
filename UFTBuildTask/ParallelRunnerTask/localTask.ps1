# 
# MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
# 
# Copyright 2016-2024 Open Text
# 
# The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
# Nothing herein should be construed as constituting an additional warranty.
# Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
# The information contained herein is subject to change without notice.
# 

using namespace PSModule.Common;
using namespace PSModule.UftMobile.SDK.UI
using namespace PSModule.UftMobile.SDK.Entity
using namespace System.Collections.Generic

param()
$testPathInput = (Get-VstsInput -Name 'testPathInput' -Require).Trim()
$timeOutIn = (Get-VstsInput -Name 'timeOutIn').Trim()
$uploadArtifact = Get-VstsInput -Name 'uploadArtifact' -Require
$artifactType = Get-VstsInput -Name 'artifactType'
$rptFileName = (Get-VstsInput -Name 'reportFileName').Trim()
[bool]$enableFailedTestsRpt = Get-VstsInput -Name 'enableFailedTestsReport' -AsBool
[string]$tsPattern = Get-VstsInput -Name 'tsPattern'

$envType = Get-VstsInput -Name 'envType'

[bool]$useChrome = Get-VstsInput -Name 'chrome' -AsBool
[bool]$useChromeH = Get-VstsInput -Name 'chromeH' -AsBool
[bool]$useChromium = Get-VstsInput -Name 'chromium' -AsBool
[bool]$useEdge = Get-VstsInput -Name 'edge' -AsBool
[bool]$useFirefox = Get-VstsInput -Name 'firefox' -AsBool
[bool]$useFirefox64 = Get-VstsInput -Name 'firefox64' -AsBool
[bool]$useIExplorer = Get-VstsInput -Name 'iExplorer' -AsBool
[bool]$useIExplorer64 = Get-VstsInput -Name 'iExplorer64' -AsBool
[bool]$useSafari = Get-VstsInput -Name 'safari' -AsBool

$uftworkdir = $env:UFT_LAUNCHER
Import-Module $uftworkdir\bin\PSModule.dll
$configs = [List[IConfig]]::new()
$configs.Add([EnvVarsConfig]::new($env:STORAGE_ACCOUNT, $env:CONTAINER))

[List[Device]]$devices = $null
if ($envType -eq "") {
	throw "Environment type not selected."
} elseif ($envType -eq "mobile") {
	$mcServerUrl = (Get-VstsInput -Name 'mcServerUrl').Trim()
	$mcDevices = (Get-VstsInput -Name 'mcDevices').Trim()
	$mcAuthType = Get-VstsInput -Name 'mcAuthType'
	$mcUsername = (Get-VstsInput -Name 'mcUsername').Trim()
	$mcPassword = Get-VstsInput -Name 'mcPassword'
	$mcAccessKey = (Get-VstsInput -Name 'mcAccessKey').Trim(' "')

	[bool]$isBasicAuth = ($mcAuthType -eq "basic")

	if ($mcDevices -eq "") {
		throw "The Devices field is required."
	} elseif ($mcServerUrl -eq "") {
		throw "Digital Lab Server is empty."
	} elseif ($isBasicAuth -and ($mcUsername -eq "")) {
		throw "Digital Lab Username is empty."
	} elseif ($isBasicAuth -and ($mcPassword.Trim() -eq "")) {
		throw "Digital Lab Password is empty."
	} elseif (!$isBasicAuth -and ($mcAccessKey -eq "")) {
		throw "Digital Lab AccessKey is empty."
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

	[bool]$useMcProxy = Get-VstsInput -Name 'useMcProxy' -AsBool
	$proxyConfig = $null

	if ($useMcProxy) {
		$mcProxyUrl = (Get-VstsInput -Name 'mcProxyUrl').Trim()
		[bool]$useMcProxyCredentials = Get-VstsInput -Name 'useMcProxyCredentials' -AsBool
		$mcProxyUsername = (Get-VstsInput -Name 'mcProxyUsername').Trim()
		$mcProxyPassword = Get-VstsInput -Name 'mcProxyPassword'
		if ($mcProxyUrl -eq "") {
			throw "Proxy Server is empty."
		} elseif ($useMcProxyCredentials -and ($mcProxyUsername -eq "")) {
			throw "Proxy Username is empty."
		} elseif ($useMcProxyCredentials -and ($mcProxyPassword.Trim() -eq "")) {
			throw "Proxy Password is empty."
		}
		$proxySrvConfig = [ServerConfig]::new($mcProxyUrl, $mcProxyUsername, $mcProxyPassword)
		$proxyConfig = [ProxyConfig]::new($proxySrvConfig, $useMcProxyCredentials)
	}
	$configs.Add([ServerConfigEx]::new($srvConfig, $useMcProxy, $proxyConfig))

	[List[string]]$invalidDeviceLines = $null
	[Device]::ParseLines($mcDevices, [ref]$devices, [ref]$invalidDeviceLines)
	if ($invalidDeviceLines -and $invalidDeviceLines.Count -gt 0) {
		foreach ($line in $invalidDeviceLines) {
			Write-Warning "Invalid device line -> $($line). The expected pattern is property1:""value1"", property2:""value2""... Valid property names are: DeviceID, Manufacturer, Model, OSType and OSVersion.";
		}
	}
	if ($devices.Count -eq 0) {
		throw "Missing or invalid devices."
	}
} elseif ($envType -eq "web") {
	$browsers = [List[string]]::new()
	if ($useChrome) {
		$browsers.Add("Chrome")
	}
	if ($useChromeH) {
		$browsers.Add("Chrome_Headless")
	}
	if ($useChromium) {
		$browsers.Add("ChromiumEdge")
	}
	if ($useEdge) {
		$browsers.Add("Edge")
	}
	if ($useFirefox) {
		$browsers.Add("Firefox")
	}
	if ($useFirefox64) {
		$browsers.Add("Firefox64")
	}
	if ($useIExplorer) {
		$browsers.Add('IE')
	}
	if ($useIExplorer64) {
		$browsers.Add('IE64')
	}
	if ($useSafari) {
		$browsers.Add('Safari')
	}
	if ($browsers.Count -eq 0) {
		throw "At least one browser is required to be selected."
	}
}
$configs.Add([ParallelRunnerConfig]::new($envType, $devices, $browsers))

# $env:SYSTEM can be used also to determine the pipeline type "build" or "release"
if ($env:SYSTEM_HOSTTYPE -eq "build") {
	$buildNumber = $env:BUILD_BUILDNUMBER
	[int]$rerunIdx = [convert]::ToInt32($env:SYSTEM_STAGEATTEMPT, 10) - 1
	$rerunType = "rerun"
} else {
	$buildNumber = $env:RELEASE_RELEASEID
	[int]$rerunIdx = $env:RELEASE_ATTEMPTNUMBER
	$rerunType = "attempt"
}

$resDir = Join-Path $uftworkdir -ChildPath "res\Report_$buildNumber"

#---------------------------------------------------------------------------------------------------

function UploadArtifactToAzureStorage($storageContext, $container, $testPathReportInput, $artifact) {
	#upload artifact to storage container
	Set-AzStorageBlobContent -Container "$($container)" -File $testPathReportInput -Blob $artifact -Context $storageContext
}

function ArchiveReport($artifact, $rptFolder) {
	if (Test-Path -LiteralPath $rptFolder) {
		$rootFolder = "$rptFolder\..\.." | Convert-Path
		$fullPathZipFile = Join-Path $rootFolder -ChildPath $artifact
		Compress-Archive -LiteralPath $rptFolder -DestinationPath $fullPathZipFile
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
	Invoke-FSTask $testPathInput $timeOutIn $uploadArtifact $artifactType $rptFileName $archiveNamePattern $buildNumber $enableFailedTestsRpt $true $configs $rptFolders $false $tsPattern -Verbose 
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
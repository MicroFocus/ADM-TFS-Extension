#
# localTask.ps1
#
using namespace PSModule.UftMobile.SDK.UI

param()
$mcServerUrl = Get-VstsInput -Name 'mcServerUrl' -Require
$mcAuthType = Get-VstsInput -Name 'mcAuthType' -Require
$mcUsername = Get-VstsInput -Name 'mcUsername'
$mcPassword = Get-VstsInput -Name 'mcPassword'
[int]$mcTenantId = Get-VstsInput -Name 'mcTenantId' -AsInt
$mcAccessKey = Get-VstsInput -Name 'mcAccessKey'
$mcResources = Get-VstsInput -Name 'mcResources' -Require
[bool]$includeOfflineDevices = Get-VstsInput -Name 'includeOfflineDevices' -AsBool

$uftworkdir = $env:UFT_LAUNCHER
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
$runStatusCodeFile = "$resDir\RunStatusCode.txt"

Import-Module $uftworkdir\bin\PSModule.dll

if ($mcAuthType -eq "basic") {
	$srvConfig = [ServerConfig]::new($mcServerUrl, $mcUsername, $mcPassword, $mcTenantId)
} else {
	$mcClientId = $mcSecret = $null
	$err = [ServerConfig]::ParseAccessKey($mcAccessKey, [ref]$mcClientId, [ref]$mcSecret, [ref]$mcTenantId)
	if ($err) {
		throw $err
	}
	$srvConfig = [ServerConfig]::new($mcServerUrl, $mcClientId, $mcSecret, $mcTenantId, $false)
}
$config = [MobileResxConfig]::new($srvConfig, $mcResources, $includeOfflineDevices, $false)

#---------------------------------------------------------------------------------------------------

$report = "$resDir\UFTM Report"

if ($rerunIdx) {
	Write-Host "$((Get-Culture).TextInfo.ToTitleCase($rerunType)) = $rerunIdx"
	if (Test-Path $report) {
		try {
			Remove-Item $report -ErrorAction Stop
		} catch {
			Write-Error "Cannot rerun because the file '$report' is currently in use."
		}
	}
}
#---------------------------------------------------------------------------------------------------
#Run the tests
Invoke-GMRTask $config $buildNumber -Verbose 

# read return code
if (Test-Path $runStatusCodeFile) {
	$content = Get-Content $runStatusCodeFile
	if ($content) {
		$sep = [Environment]::NewLine
		$option = [System.StringSplitOptions]::RemoveEmptyEntries
		$arr = $content.Split($sep, $option)
		[int]$retcode = [convert]::ToInt32($arr[-1], 10)
		if ($retcode -lt 0) {
			Write-Host "##vso[task.complete result=Failed;]"
		}
	} else {
		Write-Error "The file [$runStatusCodeFile] is empty!"
	}
} else {
	Write-Error "The file [$runStatusCodeFile] is missing!"
}

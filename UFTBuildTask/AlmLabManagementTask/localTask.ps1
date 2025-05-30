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

$uftworkdir = $env:UFT_LAUNCHER
Import-Module "$uftworkdir\bin\PSModule.dll" -ErrorAction Stop

$varAlmServ = (Get-VstsInput -Name 'varAlmserv' -Require).Trim()
[bool]$varSSOEnabled = Get-VstsInput -Name 'varSSOEnabled' -AsBool
$varClientID = (Get-VstsInput -Name 'varClientID').Trim()
$varApiKeySecret = Get-VstsInput -Name 'varApiKeySecret'
$varUserName = (Get-VstsInput -Name 'varUserName').Trim()
$varPass = Get-VstsInput -Name 'varPass'
$varDomain = (Get-VstsInput -Name 'varDomain' -Require).Trim()
$varProject = (Get-VstsInput -Name 'varProject' -Require).Trim()
$varRunType = Get-VstsInput -Name 'varRunType'
$varDescription = (Get-VstsInput -Name 'varDescription').Trim()
$varTimeslotDuration = (Get-VstsInput -Name 'varTimeslotDuration' -Require).Trim()
$varEnvironmentConfigurationID = (Get-VstsInput -Name 'varEnvironmentConfigurationID').Trim()
$varClientType = (Get-VstsInput -Name 'varClientType').Trim()
$varReportName = (Get-VstsInput -Name 'varReportName').Trim()
[string]$tsPattern = (Get-VstsInput -Name 'tsPattern').Trim()

# determine whether the entity to run is a Test Set or a Build Verification Suite
$varTSId = Get-VstsInput -Name 'varTSEntity'
$varBVSId = Get-VstsInput -Name 'varBVSEntity'

if ($varRunType -eq "TEST_SET") {
	$varEntityId = $varTSId
} else {
	$varEntityId = $varBVSId
}

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


# delete old "ALM Lab Management Report" file and create a new one
if (-Not $varReportName) {
	$varReportName = "ALM Lab Management Report"
}
$report = "$res\$varReportName"

if (Test-Path $report) {
	Remove-Item $report
}

$uftReport = "$resDir\Functional Testing Report"
$runSummary = "$resDir\Run Summary"
$retcodefile = "$resDir\TestRunReturnCode.txt"
$failedTests = "$resDir\Failed Tests"

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

Write-Host "=============================================================================="
Write-Host "$varAlmServ, Domain: $varDomain, Project: $varProject, isSSO: $varSSOEnabled, UserName: $varUserName, ClientID: $varClientID, RunType: $varRunType, EntityID: $varEntityId, Env Config ID: $varEnvironmentConfigurationID, TimeslotDuration: $varTimeslotDuration"
Write-Host "=============================================================================="
Invoke-AlmLabManagementTask $varAlmServ $varSSOEnabled $varClientID $varApiKeySecret $varUserName $varPass $varDomain $varProject $varRunType $varEntityId $varDescription $varTimeslotDuration $varEnvironmentConfigurationID $varReportName $buildNumber $varClientType $tsPattern -Verbose

#---------------------------------------------------------------------------------------------------
# uploads report files to build artifacts
$all = "$resDir\all_" + $rerunIdx
if ((Test-Path $runSummary) -and (Test-Path $uftReport)) {
	$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'
	$html = [System.Text.StringBuilder]""
	$html.Append("<div class=`"margin-right-8 margin-left-8 padding-8 depth-8`"><div class=`"body-xl`">Run Sumary</div>")
	$html.AppendLine((Get-Content -Path $runSummary))
	$html.AppendLine("</div><div class=`"margin-8 padding-8 depth-8`"><div class=`"body-xl`">Functional Testing Report</div>")
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
} else {
	Write-Error "The file [$retcodefile] is missing!"
}

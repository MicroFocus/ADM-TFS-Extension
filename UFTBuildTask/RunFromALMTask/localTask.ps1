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

$varAlmserv = (Get-VstsInput -Name 'varAlmserv' -Require).Trim()
[bool]$varSSOEnabled = Get-VstsInput -Name 'varSSOEnabled' -AsBool
$varClientID = (Get-VstsInput -Name 'varClientID').Trim()
$varApiKeySecret = Get-VstsInput -Name 'varApiKeySecret'
$varUserName = (Get-VstsInput -Name 'varUsername').Trim()
$varPass = Get-VstsInput -Name 'varPass'
$varDomain = (Get-VstsInput -Name 'varDomain' -Require).Trim()
$varProject = (Get-VstsInput -Name 'varProject' -Require).Trim()
$varTestsets = (Get-VstsInput -Name 'varTestsets' -Require).Trim()
$varTimeout = (Get-VstsInput -Name 'varTimeout').Trim()
$varReportName = (Get-VstsInput -Name 'varReportName').Trim()
$runMode = Get-VstsInput -Name 'runMode'
$testingToolHost = (Get-VstsInput -Name 'testingToolHost').Trim()
[string]$tsPattern = (Get-VstsInput -Name 'tsPattern').Trim()

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

# delete old "ALM Execution Report" file and create a new one
if (-Not $varReportName) {
	$varReportName = "ALM Execution Report"
}

$report = "$resDir\$varReportName"

if (Test-Path $report) {
	Remove-Item $report
}

$uftReport = "$resDir\UFT Report"
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

Invoke-AlmTask $varAlmserv $varSSOEnabled $varClientID $varApiKeySecret $varUserName $varPass $varDomain $varProject $varTestsets $varTimeout $varReportName $runMode $testingToolHost $buildNumber $tsPattern -Verbose

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
		} elseif ($retcode -eq -5) {
			Write-Error "ALM server is not reachable or the provided credentials/domain/project are invalid"
		} elseif ($retcode -ne 0) {
			Write-Error "Task Failed"
		}
	} else {
		Write-Error "The file [$retcodefile] is empty!"
	}
} else {
	Write-Error "The file [$retcodefile] is missing!"
}

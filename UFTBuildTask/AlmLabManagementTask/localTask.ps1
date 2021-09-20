#
# localTask.ps1
#

$varAlmServ = Get-VstsInput -Name 'varAlmserv' -Require
$varUserName = Get-VstsInput -Name 'varUserName' -Require
$varPass = Get-VstsInput -Name 'varPass'
$varDomain = Get-VstsInput -Name 'varDomain' -Require
$varProject = Get-VstsInput -Name 'varProject' -Require
$varRunType = Get-VstsInput -Name 'varRunType'
$varEntityId = Get-VstsInput -Name 'varTestSet' -Require
$varDescription = Get-VstsInput -Name 'varDescription'
$varTimeslotDuration = Get-VstsInput -Name 'varTimeslotDuration' -Require
$varClientType = Get-VstsInput -Name 'varClientType'
$varReportName = Get-VstsInput -Name 'varReportName'

$uftworkdir = $env:UFT_LAUNCHER
# $env:SYSTEM can be used also to determine the pipeline type "build" or "release"
if ($env:SYSTEM_HOSTTYPE -eq "build") {
	$buildNumber = $env:BUILD_BUILDNUMBER
	$attemptNumber = $env:SYSTEM_STAGEATTEMPT
} else {
	$buildNumber = $env:RELEASE_RELEASEID
	$attemptNumber = $env:RELEASE_ATTEMPTNUMBER
}
[int]$rerunIdx = [convert]::ToInt32($attemptNumber, 10) - 1
$resDir = Join-Path $uftworkdir -ChildPath "res\Report_$buildNumber"

Import-Module $uftworkdir\bin\PSModule.dll

# delete old "ALM Lab Management Report" file and create a new one
if (-Not $varReportName) {
	$varReportName = "ALM Lab Management Report"
}
$report = "$res\$varReportName"

if (Test-Path $report) {
	Remove-Item $report
}

$uftReport = "$resDir\UFT Report"
$runSummary = "$resDir\Run Summary"
$retcodefile = "$resDir\TestRunReturnCode.txt"
$failedTests = "$resDir\Failed Tests"

if ($rerunIdx) {
	Write-Host "Rerun attempt = $rerunIdx"
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

Invoke-AlmLabManagementTask $varAlmServ $varUserName $varPass $varDomain $varProject $varRunType $varEntityId $varDescription $varTimeslotDuration $varEnvironmentConfigurationID $varReportName $buildNumber $varClientType -Verbose

#---------------------------------------------------------------------------------------------------
# uploads report files to build artifacts
# upload and display Run Summary
if (Test-Path $runSummary) {
	if ($rerunIdx) {
		Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=Run Summary (rerun $rerunIdx);]$runSummary"
	} else {
		Write-Host "##vso[task.uploadsummary]$runSummary"
	}
}

# upload and display UFT report
if (Test-Path $uftReport) {
	if ($rerunIdx) {
		Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=UFT Report (rerun $rerunIdx);]$uftReport"
	} else {
		Write-Host "##vso[task.uploadsummary]$uftReport"
	}
}

# upload and display Failed Tests
if (Test-Path $failedTests) {
	if ($rerunIdx) {
		Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=Failed Tests (rerun $rerunIdx);]$failedTests"
	} else {
		Write-Host "##vso[task.uploadsummary]$failedTests"
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


[Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime") 
if (![Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::IsAvailable)
{
    Write-Host "Not running in fabric, exiting"
    return;
}

$drive = "C"
$prefix = "log-"
$logfolder = "$($drive):\$($prefix)$([Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::CurrentRoleInstance.Id)"
$logfilename = "$($logfolder)\$([System.DateTime]::UtcNow.ToString("yyyy-MM.dd--HH-mm-ss-fff")).txt"

if (-not (Test-Path -Path $logfolder)) 
{ 
    [void] (New-Item -ItemType Directory -Path $logfolder ) 
}
[System.Environment]::SetEnvironmentVariable("$logfolder", $logfolder)
[System.Environment]::SetEnvironmentVariable("logfilename", $logfilename)
Start-Process -NoNewWindow -Wait -FilePath "$($Env:windir)\System32\cmd.exe" -ArgumentList "/C $(Get-Location)\install2.cmd"

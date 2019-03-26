Write-Host "Running setup code"

$logfolder = [System.Environment]::GetEnvironmentVariable("$logfolder")

[Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime") 
if (![Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::IsAvailable)
{
    Write-Host "Not running in fabric, exiting"
    return;
}

Function Enable-ReceiveSideScaling
{
	$numberOfCores = (Get-WmiObject -Class win32_processor -Property "numberOfCores" | Select-Object -Property "numberOfCores").numberOfCores
	Write-Host "The number of cores is $($numberOfCores)"

	$NumberOfLogicalProcessors = (Get-WmiObject -Class win32_processor -Property "NumberOfLogicalProcessors" | Select-Object -Property "NumberOfLogicalProcessors").NumberOfLogicalProcessors
	Write-Host "The number of logical processors is $($NumberOfLogicalProcessors)"

	$rssAvailableOnVM = (Get-NetAdapterAdvancedProperty -Name Ethernet | where DisplayName -eq "Receive Side Scaling") -ne $null
	if ($rssAvailableOnVM)
	{
		Write-Host "Receive Side Scaling is available on the network driver"
		$rssAlreadyEnabled = (Get-NetAdapterAdvancedProperty -Name Ethernet | where DisplayName -eq "Receive Side Scaling").DisplayValue -eq "Enabled"
		if ($rssAlreadyEnabled) 
		{
			Write-Host "Receive Side Scaling is already turned on"
		}
		else 
		{
			Write-Host "Receive Side Scaling is not turned on, will turn on now"

			# Set-NetAdapterRss -Name Ethernet -Enabled $True -NoRestart
			# Set-NetAdapterRss -Name Ethernet -Enabled $True -BaseProcessorNumber 0 -MaxProcessors $numberOfCores -NoRestart
			Set-NetAdapterRss -Name Ethernet -Enabled $True -BaseProcessorNumber 0 -MaxProcessors $numberOfCores -NumberOfReceiveQueues $numberOfCores
		}
	}
}

Function Set-NetworkingParameters
{
	Enable-ReceiveSideScaling 
}

if (-not $emulated)
{
	$restartRequired = (-not $(Test-Path HKCU:\Software\AzureStartup)) 

	Set-NetworkingParameters

	if ($restartRequired) 
	{ 
		New-Item -Path HKCU:\Software\AzureStartup
		Set-ItemProperty -Path HKCU:\Software\AzureStartup -Type "DWord" -Name "RunAlready" -Value 1 
		Restart-Computer
	}
}

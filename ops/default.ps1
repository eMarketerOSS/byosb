
Framework "4.6"

properties {
  $root = Resolve-Path ..
  $sln = "$root\Dropin.UnicastBus.sln"
  $proj = "$root\host.inazure\host.inazure.ccproj"
  $sourceCscfgFile = "$root\host.inazure\ServiceConfiguration.Cloud.cscfg"
  $build = "$root\build"
  $cscfg = "$root\build\ServiceConfiguration.Cloud.cscfg"
  $cspkg = "$root\build\host.inazure.cspkg"
}

task default -depends Deploy

task PrepareConfig -depends BuildPackage {

	$conn = (Get-ChildItem Env:shuttle-sb-connection).Value

	"Build: $build"
	"Sln: $sln"
	"Where: $root"
	"Source Cfg: $sourceCscfgFile"
	"Bus conn: $conn"


	[Xml]$cscfgXml = Get-Content $sourceCscfgFile

	Foreach ($role in $cscfgXml.ServiceConfiguration.Role)
    {
        Foreach ($setting in $role.ConfigurationSettings.Setting)
        {
            Switch ($setting.name)
            {
                "shuttle-sb-connection" {$setting.value = $conn}
            }
        }
    }

    $cscfgXml.InnerXml | Out-File -Encoding utf8 $cscfg
}

task BuildPackage { 
  
  	"Pakaged here: $build"

	exec { msbuild $proj /p:Configuration=Release /p:PublishDir=$build\ /p:TargetProfile=Cloud /t:publish /verbosity:quiet /nologo }

}

task Deploy -depends PrepareConfig {

	$cloudServiceName = "s100002"
	$storageAccountName = "{0}storage" -f $cloudServiceName
	$Location = "West US"

	if (!(Test-AzureName -Storage $storageAccountName))
	{  
	    Write-Host "Creating Storage Account $storageAccountName"
		New-AzureStorageAccount -StorageAccountName $storageAccountName -Location $Location -Verbose
	}

	if (!(Test-AzureName -Service $cloudServiceName))
	{  
	    Write-Host "Creating Cloud Service $cloudServiceName"
		New-AzureService -ServiceName $cloudServiceName -Location $Location
	}


	#Add-AzureAccout and make it current

	$s = Get-AzureSubscription -Current
	Set-AzureSubscription -SubscriptionName $s.SubscriptionName -CurrentStorageAccount $storageAccountName


	$deployment = $null
	Try
	{
	    $deployment = Get-AzureDeployment -ServiceName $cloudServiceName
	}
	Catch
	{
	    New-AzureDeployment -ServiceName $cloudServiceName -Slot Production -Configuration $cscfg -Package $cspkg
	}
	If ($deployment)
	{
	    Set-AzureDeployment -ServiceName $cloudServiceName -Slot Production -Configuration $cscfg -Package $cspkg -Mode Simultaneous -Upgrade
	}

	# Mark the finish time of the script execution
	$finishTime = Get-Date
	# Output the time consumed in seconds
	Write-Output ("Total time used (seconds): {0}" -f ($finishTime - $startTime).TotalSeconds)

	# Launch the browser to show the website
	If ($Launch)
	{
	    Start-Process -FilePath ("http://{0}.cloudapp.net" -f $cloudServiceName)
	}
}

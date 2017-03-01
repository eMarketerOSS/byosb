
Framework "4.6"

properties {
  $root = Resolve-Path ..
  $sln = "$root\Dropin.UnicastBus.sln"
  $proj = "$root\host.inazure\host.inazure.ccproj"
  $sourceCscfgFile = "$root\host.inazure\ServiceConfiguration.Cloud.cscfg"
  $build = "$root\build"
  $cscfg = "$root\build\ServiceConfiguration.Cloud.cscfg"
  $temp = "$root\build\temp"
}

task default -depends PrepareConfig

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

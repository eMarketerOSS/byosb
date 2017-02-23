
properties {
  $root = Resolve-Path ..
  $sln = "$root\Dropin.UnicastBus.sln"
  $proj = "$root\host.inazure\host.inazure.ccproj"
  $build = "$root\build"
}

task default -depends Build

task Dev {
	"Build: $build"
	"Sln: $sln"
	"Where: $root"
}

task Build { 
  
	exec { msbuild $proj /p:Configuration=Release 
                        /p:DebugType=None
                        /p:Platform=AnyCpu
                        /p:OutputPath=$build\
                        /p:TargetProfile=Cloud
                        /t:publish
                        /verbosity:minimal }

}

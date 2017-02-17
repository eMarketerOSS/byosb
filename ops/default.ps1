
properties {
  $root = "..\"
  $sln = $root + "Dropin.UnicastBus.sln"
}

task default -depends Build

task Build { 
  
	exec { msbuild $sln /p:Configuration=Release 
                        /p:DebugType=None
                        /p:Platform=AnyCpu
                        /p:OutputPath=$root
                        /p:TargetProfile=Cloud
                        /p:VisualStudioVersion=12.0
                        /t:publish
                        /verbosity:quiet }
}

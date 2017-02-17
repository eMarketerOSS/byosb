
properties {
  $root = "C:\Users\Ruslan\Documents\byosb"
  $sln = "$root\Dropin.UnicastBus.sln"
}

task default -depends Build

task Build { 
  
	exec { msbuild $sln /p:Configuration=Release 
                        /p:DebugType=None
                        /p:Platform=AnyCpu
                        /p:OutputPath=<path-to-package>
                        /p:TargetProfile=Cloud
                        /t:publish
                        /verbosity:quiet }
}

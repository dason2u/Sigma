@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)
 
set version=1.0.0
if not "%PackageVersion%" == "" (
   set version=%PackageVersion%
)

set nuget=
if "%nuget%" == "" (
	set nuget=nuget
)

set build="%programfiles(x86)%\MSBuild\14.0\Bin\MsBuild.exe"

powershell -Command "(new-object System.Net.WebClient).DownloadFile('https://dist.nuget.org/win-x86-commandline/latest/nuget.exe','nuget.exe')"

copy nuget.exe Sigma.Core
copy nuget.exe Sigma.Core.Monitors.WPF

REM build Sigma.Core
cd Sigma.Core
	
	nuget.exe restore -SolutionDirectory ../

	%build% Sigma.Core.csproj /p:Configuration="%config%" /p:Platform=x64

	REM pack Sigma.Core

	nuget.exe pack "Sigma.Core.csproj" -IncludeReferencedProjects -Prop Platform=x64 -Verbosity detailed -Prop Configuration=%config% -Version %version%


REM build Sigma.Core.Monitors.WPF
cd ../Sigma.Core.Monitors.WPF

	REM update the targets file
	cd build
		REM this is required in order to handle it as text instead of binary (im a powershell noob)
		move Sigma.Core.Monitors.WPF.template.targets targets.txt
		powershell -Command "(Get-Content targets.txt) -replace '~version~', '%version%' | Set-Content Sigma.Core.Monitors.WPF.targets.txt"
		move Sigma.Core.Monitors.WPF.targets.txt Sigma.Core.Monitors.WPF.targets
		move targets.txt Sigma.Core.Monitors.WPF.template.targets
		cd ..
		
	REM actual build
	nuget.exe restore -SolutionDirectory ../

	%build% Sigma.Core.Monitors.WPF.csproj /p:Configuration="%config%" /p:Platform=x64
	
	REM pack Sigma.Core.Monitors.WPF
	
	nuget.exe pack "Sigma.Core.Monitors.WPF.csproj" -IncludeReferencedProjects -Prop Platform=x64 -Verbosity detailed -Prop Configuration=%config% -Version %version%
	
	
	REM cleanup
	
	REM delete newly created target file
	cd build 
		del Sigma.Core.Monitors.WPF.targets
	cd ..
	del nuget.exe
	cd ..
	del nuget.exe
	cd Sigma.Core
	del nuget.exe
	cd ..
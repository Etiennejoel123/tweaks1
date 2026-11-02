# Local build, test and publish. Run from the repository root on Windows with the .NET 10 SDK.
$ErrorActionPreference = 'Stop'
dotnet restore StrohmayerTweaks.sln
dotnet build StrohmayerTweaks.sln -c Release --no-restore
dotnet test StrohmayerTweaks.sln -c Release --no-build
dotnet publish src/StrohmayerTweaks.App/StrohmayerTweaks.App.csproj -c Release -r win-x64 --self-contained true -o publish
Compress-Archive -Path publish\* -DestinationPath Strohmayer-Tweaks-Windows-x64.zip -Force
Write-Host 'Done: Strohmayer-Tweaks-Windows-x64.zip'

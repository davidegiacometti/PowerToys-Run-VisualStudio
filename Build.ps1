$ErrorActionPreference = "Stop"

[xml]$xml = Get-Content -Path "$PSScriptRoot\Directory.Build.Props"
$version = $xml.Project.PropertyGroup.Version

foreach ($platform in "ARM64", "x64")
{
    if (Test-Path -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.VisualStudio\bin")
    {
        Remove-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.VisualStudio\bin\*" -Recurse
    }

    if (Test-Path -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.VisualStudio\obj")
    {
        Remove-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.VisualStudio\obj\*" -Recurse
    }

    dotnet build $PSScriptRoot\Community.PowerToys.Run.Plugin.VisualStudio.sln -c Release /p:Platform=$platform

    Remove-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.VisualStudio\bin\*" -Recurse -Include *.xml, *.pdb, PowerToys.*, Wox.*
    Rename-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.VisualStudio\bin\$platform\Release" -NewName "VisualStudio"

    Compress-Archive -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.VisualStudio\bin\$platform\VisualStudio" -DestinationPath "$PSScriptRoot\VisualStudio-$version-$platform.zip"
}

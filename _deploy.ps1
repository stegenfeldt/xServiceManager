$modDir = [System.IO.FileInfo] '.\xServiceManager'
$currentManifest = $(Get-Content -Path "$($modDir.FullName)\xServiceManager.psd1" | Out-String) | Invoke-Expression
$version = $currentManifest.ModuleVersion -as [version]
$version = [version]::new($version.Major, $version.Minor, $version.Build, $version.Revision + 1)
$Public = @( Get-ChildItem -Path "$($modDir.FullName)\Public\*.ps1" -ErrorAction SilentlyContinue )
$Parms = @{
    Path              = "$($modDir.FullName)\xServiceManager.psd1"
    Guid              = '745e34ef-0024-4e7b-a382-30b892536374'
    Author            = 'stegenfeldt'
    CompanyName       = 'TeknoglotSE'
    Copyright         = '(c) 2022 TeknoglotSE. All rights reserved.'
    ModuleVersion     = $version
    FunctionsToExport = $Public.Basename
    Description       = 'Support-module to the stock ServiceManager module without reliance on 3rd-party DLLs'
}
Update-ModuleManifest @Parms

Publish-Module -Path 'D:\dev\xServiceManager\xServiceManager' -Repository 'PSRepo-TEST' -NuGetApiKey 'xServiceManager' -Force -Verbose
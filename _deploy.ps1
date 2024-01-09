$modDir = [System.IO.FileInfo] '.\xServiceManager'
$currentManifest = $(Get-Content -Path "$($modDir.FullName)\xServiceManager.psd1" | Out-String) | Invoke-Expression
$version = $currentManifest.ModuleVersion -as [version]
$version = [version]::new($version.Major, $version.Minor, $version.Build, $version.Revision + 1)
$Public = @( Get-ChildItem -Path "$($modDir.FullName)\Public\*.ps1" -ErrorAction SilentlyContinue )
$Parms = @{
    Path               = "$($modDir.FullName)\xServiceManager.psd1"
    RootModule         = "xServiceManager.Module.dll"
    Guid               = '745e34ef-0024-4e7b-a382-30b892536374'
    Author             = 'stegenfeldt, Sundqvist, Truher, Wright, Gritsenko'
    CompanyName        = 'Community Developed'
    Copyright          = 'Copyright 2022.'
    ModuleVersion      = $version
    FunctionsToExport  = $Public.Basename
    Description        = 'Support-module to the stock ServiceManager module without clobbering the default modules. Based on SMLets. Should support Windows Powershell 5.1+ and Powershell 7+.'
    NestedModules      = @('xServiceManager.psm1')
}

Update-ModuleManifest @Parms

Publish-Module -Path $modDir.FullName -Repository 'PSRepo-TEST' -NuGetApiKey 'xServiceManager' -Force -Verbose
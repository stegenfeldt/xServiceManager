[System.IO.FileInfo] $currentFile = $MyInvocation.MyCommand.Path -as [System.IO.FileInfo]
[System.IO.FileInfo] $wsDir = $currentFile.DirectoryName -as [System.IO.FileInfo]
[System.IO.FileInfo] $modDir = "$($wsDir.FullName)\xServiceManager" -as [System.IO.FileInfo] 
[string] $modManifestPath = "$($modDir.FullName)\xServiceManager.psd1" -as [string]
$currentManifest = $(Get-Content -Path $modManifestPath | Out-String) | Invoke-Expression
$version = $currentManifest.ModuleVersion -as [version]
$version = [version]::new($version.Major, $version.Minor, $version.Build, $version.Revision + 1)
$Public = @( Get-ChildItem -Path "$($modDir.FullName)\Public\*.ps1" -ErrorAction SilentlyContinue )
$Parms = @{
    Path               = $modManifestPath
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

[bool] $updateDoc = $false
if ($null -eq (Get-Module -Name 'platyPS')) {
    if ($null -ne (Get-Module -Name 'platyPS' -ListAvailable)) {
        Import-Module -Name 'platyPS'
        if ($?) {
            $updateDoc = $true
        }
    } else {
        # module is not available, attempt to install it for the current user
        Install-Module -Name 'platyPS' -Scope CurrentUser -AcceptLicense -Confirm -SkipPublisherCheck -Repository PSGallery
        Import-Module -Name 'platyPS'
        if ($?) {
            $updateDoc = $true
        }
    }
} else {
    $updateDoc = $true
}

if ($updateDoc) {
    Import-Module -Name $modManifestPath -Force
    $docParams = @{
        #Module = 'xServiceManager'
        Path = "$wsDir\docs"
        UseFullTypeName = $true
        UpdateInputOutput = $true
        ExcludeDontShow = $true
        #RefreshModulePage = $true
    }
    Update-MarkdownHelp @docParams
}


Publish-Module -Path $modDir.FullName -Repository 'PSRepo-TEST' -NuGetApiKey 'xServiceManager' -Force -Verbose
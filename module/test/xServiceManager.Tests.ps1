$ModuleManifestName = 'xServiceManager.psd1'
$ModuleManifestPath = "$PSScriptRoot\..\$ModuleManifestName"

Describe 'Module Manifest Tests' {
    It 'Passes Test-ModuleManifest' {
        Test-ModuleManifest -Path $ModuleManifestPath | Should Not BeNullOrEmpty
        $? | Should Be $true
    }
}

Describe 'Loading Module Tests' {
    It 'Loads xServiceManager module' {
        Import-Module -$ModuleManifestPath
        $moduleLoaded = Get-Module -Name 'xServiceManager'
        $moduleLoaded | Should Be $true
    }
}
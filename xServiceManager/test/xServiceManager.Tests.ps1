
Describe 'Module Manifest Tests' {
    It 'Passes Test-ModuleManifest' {
        $ModuleManifestName = 'xServiceManager.psd1'
        $ModuleManifestPath = "$PSScriptRoot\..\$ModuleManifestName"    
        Test-ModuleManifest -Path $ModuleManifestPath | Should -Not -BeNullOrEmpty
        $? | Should -Be $true
    }
}

Describe 'Loading Module Tests' {
    It 'Loads xServiceManager module' {
        $ModuleManifestName = 'xServiceManager.psd1'
        $ModuleManifestPath = "$PSScriptRoot\..\$ModuleManifestName"
        Import-Module -Name $ModuleManifestPath
        $moduleLoaded = Get-Module -Name 'xServiceManager'
        $moduleLoaded | Should -Be $true
    }
}
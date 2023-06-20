BeforeAll {
    Import-Module -Name "$PSScriptRoot\..\xServiceManager.psd1"
}

AfterAll {
    Remove-Module -Name 'xServiceManager'
}

Describe "Get-xSCSMClass" {
    Context "When no parameters are provided" {
        It "Should return no classes" {
            $result = Get-xSCSMClass

            $result | Should -BeNullOrEmpty
        }
    }

    Context "When a specific class name is provided" {
        It "Should return the specified class" {
            $result = Get-xSCSMClass -Name "System.Entity"

            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be "System.Entity"
        }
    }

    Context "When a wildcard is used" {
        It "Should return the specified class" {
            $result = Get-xSCSMClass -Name "Microsoft.Windows.Server.*"

            $result | Should -Not -BeNullOrEmpty
            $result.Count | Should -BeGreaterThan 1
        }
    }

    Context "When an invalid class name is provided" {
        It "Should return no classes" {
            $result = Get-xSCSMClass -Name "InvalidClassName"
            $result | Should -BeNullOrEmpty
        }
    }
}

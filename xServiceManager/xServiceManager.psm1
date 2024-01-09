# load global module variables
$GLOBAL:XSCSMSMADLL = ([System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.location -match 'System.Management.Automation.dll' }).location
$GLOBAL:XSCSMINSTALLDIR = (Get-ItemProperty 'HKLM:/Software/Microsoft/System Center/2010/Service Manager/Setup').InstallDirectory
$GLOBAL:XSCSMSDKDIR = "${XSCSMINSTALLDIR}\SDK Binaries"
$GLOBAL:XSCSMCOREDLL = "${XSCSMSDKDIR}\Microsoft.EnterpriseManagement.Core.dll"
$GLOBAL:XSCSMSMDLL = "${XSCSMSDKDIR}\Microsoft.EnterpriseManagement.ServiceManager.dll"
$GLOBAL:XSCSMPACKAGINGDLL = "${XSCSMSDKDIR}\Microsoft.EnterpriseManagement.Packaging.dll"
$GLOBAL:XSCSMEMGTYPE = 'Microsoft.EnterpriseManagement.EnterpriseManagementGroup'


#Get public and private function definition files.
$Public = @( Get-ChildItem -Path $PSScriptRoot\Public\*.ps1 -ErrorAction SilentlyContinue )
$Private = @( Get-ChildItem -Path $PSScriptRoot\Private\*.ps1 -ErrorAction SilentlyContinue )

#Dot source the files
Foreach ($import in @($Public + $Private)) {
    Try {
        . $import.fullname
    } Catch {
        Write-Error -Message "Failed to import function $($import.fullname): $_"
    }
}

Export-ModuleMember -Function $Public.Basename

# prepare global module stuff
if ($null -eq (Get-Module -Name 'System.Center.Service.Manager')) {
    Import-Module -Name "$GLOBAL:XSCSMINSTALLDIR\Powershell\System.Center.Service.Manager.psd1" -Force
}
Import-xSCSMAssembly -AssemblyFile $GLOBAL:XSCSMCOREDLL
Import-xSCSMAssembly -AssemblyFile $GLOBAL:XSCSMSMDLL
Import-xSCSMAssembly -AssemblyFile $GLOBAL:XSCSMPACKAGINGDLL

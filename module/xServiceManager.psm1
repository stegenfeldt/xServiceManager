# load global module variables
$GLOBAL:XSCSMSMADLL = ([System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.location -match 'System.Management.Automation.dll' }).location
$GLOBAL:XSCSMINSTALLDIR = (Get-ItemProperty 'HKLM:/Software/Microsoft/System Center/2010/Service Manager/Setup').InstallDirectory
$GLOBAL:XSCSMSDKDIR = "${XSCSMINSTALLDIR}\SDK Binaries"
$GLOBAL:XSCSMCOREDLL = "${XSCSMSDKDIR}/Microsoft.EnterpriseManagement.Core.dll"
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
Import-xSCSMAssembly -AssemblyFile $GLOBAL:XSCSMCOREDLL

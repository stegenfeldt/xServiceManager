$ModuleManifestName = 'xServiceManager.psd1'
$ModuleManifestPath = "$PSScriptRoot\..\$ModuleManifestName"

if (Get-Module -Name 'SMLets') { Remove-Module -Name 'SMLets' }
Import-Module -Name $ModuleManifestPath -Force

# Connect to 'localhost' MG
if (!(Get-SCManagementGroupConnection)) { New-SCManagementGroupConnection -ComputerName 'localhost' }
# Get-xSCSMEnumeration -Name '*Closed*' | Format-Table Name, DisplayName
# Get-xSCSMEnumeration -DisplayName '*Pågår*' | Format-Table Name, DisplayName
# Get-xSCSMEnumeration -All | Format-Table Name, DisplayName

$class = Get-SCSMClass -Name 'RG.AssetManagement.Asset'
$asset = Get-SCSMClassInstance -Class $class -Filter 'AssetId -eq 10004'
$relObjs = Get-xSCSMRelatedObject -SourceObject $asset
$relObj = $relObjs[0].TargetObject

Get-xSCSMRelatedObject -TargetObject $relObj
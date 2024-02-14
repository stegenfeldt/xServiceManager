---
external help file: xServiceManager.Module.dll-Help.xml
Module Name: xServiceManager
online version:
schema: 2.0.0
---

# Get-xSCSMObjectProjection

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### Wrapped (Default)
```
Get-xSCSMObjectProjection [-ProjectionObject] <PSObject> [-NoSort] [-NoCommit] [-Filter <String>]
 [-MaxCount <Int32>] [-SortBy <String>] [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>]
 [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [<CommonParameters>]
```

### Statistics
```
Get-xSCSMObjectProjection [-ProjectionObject] <PSObject> [-NoSort] [-Statistic] [-NoCommit] [-Filter <String>]
 [-MaxCount <Int32>] [-SortBy <String>] [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>]
 [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [<CommonParameters>]
```

### Raw
```
Get-xSCSMObjectProjection [-Projection] <ManagementPackTypeProjection> [-NoSort] [-AdoptWithTargetEndpoint]
 [-NoCommit] [-Filter <String>] [-MaxCount <Int32>] [-SortBy <String>] [-QueryOption <ObjectQueryOptions>]
 [-ComputerName <String>] [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [<CommonParameters>]
```

### Name
```
Get-xSCSMObjectProjection [-ProjectionName] <String> [-NoSort] [-AdoptWithTargetEndpoint] [-NoCommit]
 [-Filter <String>] [-MaxCount <Int32>] [-SortBy <String>] [-QueryOption <ObjectQueryOptions>]
 [-ComputerName <String>] [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [<CommonParameters>]
```

### Criteria
```
Get-xSCSMObjectProjection -Criteria <ObjectProjectionCriteria> [-NoSort] [-AdoptWithTargetEndpoint] [-NoCommit]
 [-Filter <String>] [-MaxCount <Int32>] [-SortBy <String>] [-QueryOption <ObjectQueryOptions>]
 [-ComputerName <String>] [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -AdoptWithTargetEndpoint
{{ Fill AdoptWithTargetEndpoint Description }}

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: Raw, Name, Criteria
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ComputerName
The computer to use for the connection to the Service Manager Data Access Service

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Credential
{{ Fill Credential Description }}

```yaml
Type: System.Management.Automation.PSCredential
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Criteria
{{ Fill Criteria Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Common.ObjectProjectionCriteria
Parameter Sets: Criteria
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Filter
{{ Fill Filter Description }}

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaxCount
{{ Fill MaxCount Description }}

```yaml
Type: System.Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoCommit
{{ Fill NoCommit Description }}

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoSort
{{ Fill NoSort Description }}

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Projection
{{ Fill Projection Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPackTypeProjection
Parameter Sets: Raw
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -ProjectionName
{{ Fill ProjectionName Description }}

```yaml
Type: System.String
Parameter Sets: Name
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProjectionObject
{{ Fill ProjectionObject Description }}

```yaml
Type: System.Management.Automation.PSObject
Parameter Sets: Wrapped, Statistics
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -QueryOption
{{ Fill QueryOption Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Common.ObjectQueryOptions
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SCSMSession
A connection to a Service Manager Data Access Service

```yaml
Type: Microsoft.EnterpriseManagement.EnterpriseManagementGroup
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SortBy
{{ Fill SortBy Description }}

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Statistic
{{ Fill Statistic Description }}

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: Statistics
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ThreeLetterWindowsLanguageName
Language code for connection.
The default is current UI Culture

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

### Microsoft.EnterpriseManagement.Configuration.ManagementPackTypeProjection

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS

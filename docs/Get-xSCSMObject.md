---
external help file: xServiceManager.Module.dll-Help.xml
Module Name: xServiceManager
online version:
schema: 2.0.0
---

# Get-xSCSMObject

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### Class (Default)
```
Get-xSCSMObject [-Class] <ManagementPackClass> [-NoAdapt] [-Filter <String>] [-MaxCount <Int32>]
 [-SortBy <String>] [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>] [-Credential <PSCredential>]
 [-SCSMSession <EnterpriseManagementGroup>] [-ThreeLetterWindowsLanguageName <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Statistic
```
Get-xSCSMObject [-Class] <ManagementPackClass> [-NoAdapt] [-Statistic] [-Filter <String>] [-MaxCount <Int32>]
 [-SortBy <String>] [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>] [-Credential <PSCredential>]
 [-SCSMSession <EnterpriseManagementGroup>] [-ThreeLetterWindowsLanguageName <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Guid
```
Get-xSCSMObject [-Id] <Guid> [-NoAdapt] [-Filter <String>] [-MaxCount <Int32>] [-SortBy <String>]
 [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>] [-Credential <PSCredential>]
 [-SCSMSession <EnterpriseManagementGroup>] [-ThreeLetterWindowsLanguageName <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Criteria
```
Get-xSCSMObject -Criteria <EnterpriseManagementObjectCriteria> [-NoAdapt] [-Filter <String>]
 [-MaxCount <Int32>] [-SortBy <String>] [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>]
 [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
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

### -Class
{{ Fill Class Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPackClass
Parameter Sets: Class, Statistic
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
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
Type: Microsoft.EnterpriseManagement.Common.EnterpriseManagementObjectCriteria
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

### -Id
{{ Fill Id Description }}

```yaml
Type: System.Guid
Parameter Sets: Guid
Aliases:

Required: True
Position: 0
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

### -NoAdapt
{{ Fill NoAdapt Description }}

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
Parameter Sets: Statistic
Aliases:

Required: False
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

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: System.Management.Automation.ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### Microsoft.EnterpriseManagement.Configuration.ManagementPackClass

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS

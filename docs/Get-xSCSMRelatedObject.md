---
external help file: xServiceManager.Module.dll-Help.xml
Module Name: xServiceManager
online version:
schema: 2.0.0
---

# Get-xSCSMRelatedObject

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

```
Get-xSCSMRelatedObject [-SMObject] <EnterpriseManagementObject> [-Relationship <ManagementPackRelationship>]
 [-Depth <TraversalDepth>] [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>]
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

### -Depth
{{ Fill Depth Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Common.TraversalDepth
Parameter Sets: (All)
Aliases:
Accepted values: OneLevel, Recursive

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

### -Relationship
{{ Fill Relationship Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPackRelationship
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

### -SMObject
{{ Fill SMObject Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
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

### Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS

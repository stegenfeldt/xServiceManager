---
external help file: xServiceManager.Module.dll-Help.xml
Module Name: xServiceManager
online version:
schema: 2.0.0
---

# New-xSCSMView

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

```
New-xSCSMView [-Folder] <ManagementPackFolder> [[-DisplayName] <String>] [[-Class] <ManagementPackClass>]
 [[-ManagementPack] <ManagementPack>] [-Criteria] <String> [[-Columns] <Column[]>]
 [[-Image] <ManagementPackImage>] [[-ManagementPackReference] <KeyValuePair`2[]>]
 [[-Projection] <ManagementPackTypeProjection>] [-PassThru] [-QueryOption <ObjectQueryOptions>]
 [-ComputerName <String>] [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
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
The target class of the view.

```yaml
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPackClass
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Columns
The columns to show.

```yaml
Type: xServiceManager.Module.Column[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 5
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
The view criteria.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: True
Position: 4
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -DisplayName
The display name of the view.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Folder
The folder of the view.

```yaml
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPackFolder
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Image
The tree image for the view

```yaml
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPackImage
Parameter Sets: (All)
Aliases:

Required: False
Position: 6
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ManagementPack
The management pack to store the view in.

```yaml
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPack
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ManagementPackReference
The management pack references to add to be used in the criteria XML

```yaml
Type: System.Collections.Generic.KeyValuePair`2[System.String,Microsoft.EnterpriseManagement.Configuration.ManagementPackReference][]
Parameter Sets: (All)
Aliases:

Required: False
Position: 7
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
{{ Fill PassThru Description }}

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
The type projection to be used.

```yaml
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPackTypeProjection
Parameter Sets: (All)
Aliases:

Required: False
Position: 8
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

### Microsoft.EnterpriseManagement.Configuration.ManagementPackFolder

### System.String

### Microsoft.EnterpriseManagement.Configuration.ManagementPackClass

### Microsoft.EnterpriseManagement.Configuration.ManagementPack

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS

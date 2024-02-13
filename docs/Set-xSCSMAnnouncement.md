---
external help file: xServiceManager.Module.dll-Help.xml
Module Name: xServiceManager
online version:
schema: 2.0.0
---

# Set-xSCSMAnnouncement

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### ById
```
Set-xSCSMAnnouncement [[-DisplayName] <String>] [[-Body] <String>] [[-Priority] <String>]
 [[-ExpirationDate] <DateTime>] [-InternalID] <String> [-PassThru] [-ComputerName <String>]
 [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### ByObject
```
Set-xSCSMAnnouncement [[-DisplayName] <String>] [[-Body] <String>] [[-Priority] <String>]
 [[-ExpirationDate] <DateTime>] -Announcement <EnterpriseManagementObject> [-PassThru] [-ComputerName <String>]
 [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
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

### -Announcement
{{ Fill Announcement Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject
Parameter Sets: ByObject
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Body
The body of the announcement

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
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

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: cf

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

### -DisplayName
The title of the announcement.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExpirationDate
The expiration date of the announcement. 
Pass a datetime object. 
Convert to UTC time first.

```yaml
Type: System.Nullable`1[System.DateTime]
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InternalID
The internal ID (GUID) of the announcement.

```yaml
Type: System.String
Parameter Sets: ById
Aliases:

Required: True
Position: 4
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

### -Priority
The priority of the announcement. 
Must be exactly 'Low', 'Medium', or 'Critical'.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:
Accepted values: Low, Medium, Critical

Required: False
Position: 2
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

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: wi

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

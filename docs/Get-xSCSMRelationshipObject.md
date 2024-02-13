---
external help file: xServiceManager.Module.dll-Help.xml
Module Name: xServiceManager
online version:
schema: 2.0.0
---

# Get-xSCSMRelationshipObject

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### ID (Default)
```
Get-xSCSMRelationshipObject [-Id] <Guid> [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>]
 [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### RELATIONSHIP
```
Get-xSCSMRelationshipObject [-Relationship] <ManagementPackRelationship[]>
 [-ByTarget <EnterpriseManagementObject>] [-BySource <EnterpriseManagementObject>] [-Filter <String>]
 [-Recursive <Boolean>] [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>]
 [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### TARGETANDRELATIONSHIP
```
Get-xSCSMRelationshipObject -TargetRelationship <ManagementPackRelationship>
 -TargetObject <EnterpriseManagementObject> [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>]
 [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### TARGET
```
Get-xSCSMRelationshipObject -Target <ManagementPackClass> [-Filter <String>] [-Recursive <Boolean>]
 [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>] [-Credential <PSCredential>]
 [-SCSMSession <EnterpriseManagementGroup>] [-ThreeLetterWindowsLanguageName <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### SOURCE
```
Get-xSCSMRelationshipObject -Source <ManagementPackClass> [-Filter <String>] [-Recursive <Boolean>]
 [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>] [-Credential <PSCredential>]
 [-SCSMSession <EnterpriseManagementGroup>] [-ThreeLetterWindowsLanguageName <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### TARGETOBJECT
```
Get-xSCSMRelationshipObject -ByTarget <EnterpriseManagementObject> [-QueryOption <ObjectQueryOptions>]
 [-ComputerName <String>] [-Credential <PSCredential>] [-SCSMSession <EnterpriseManagementGroup>]
 [-ThreeLetterWindowsLanguageName <String>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### SOURCEOBJECT
```
Get-xSCSMRelationshipObject -BySource <EnterpriseManagementObject> [-Filter <String>] [-Recursive <Boolean>]
 [-QueryOption <ObjectQueryOptions>] [-ComputerName <String>] [-Credential <PSCredential>]
 [-SCSMSession <EnterpriseManagementGroup>] [-ThreeLetterWindowsLanguageName <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### FILTER
```
Get-xSCSMRelationshipObject -Filter <String> [-Recursive <Boolean>] [-QueryOption <ObjectQueryOptions>]
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

### -BySource
{{ Fill BySource Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject
Parameter Sets: RELATIONSHIP
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject
Parameter Sets: SOURCEOBJECT
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ByTarget
{{ Fill ByTarget Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject
Parameter Sets: RELATIONSHIP
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject
Parameter Sets: TARGETOBJECT
Aliases:

Required: True
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

### -Filter
{{ Fill Filter Description }}

```yaml
Type: System.String
Parameter Sets: RELATIONSHIP, TARGET, SOURCE, SOURCEOBJECT
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: System.String
Parameter Sets: FILTER
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
{{ Fill Id Description }}

```yaml
Type: System.Guid
Parameter Sets: ID
Aliases:

Required: True
Position: 0
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

### -Recursive
{{ Fill Recursive Description }}

```yaml
Type: System.Boolean
Parameter Sets: RELATIONSHIP, TARGET, SOURCE, SOURCEOBJECT, FILTER
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
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPackRelationship[]
Parameter Sets: RELATIONSHIP
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
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

### -Source
{{ Fill Source Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPackClass
Parameter Sets: SOURCE
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Target
{{ Fill Target Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPackClass
Parameter Sets: TARGET
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TargetObject
{{ Fill TargetObject Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject
Parameter Sets: TARGETANDRELATIONSHIP
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TargetRelationship
{{ Fill TargetRelationship Description }}

```yaml
Type: Microsoft.EnterpriseManagement.Configuration.ManagementPackRelationship
Parameter Sets: TARGETANDRELATIONSHIP
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

### Microsoft.EnterpriseManagement.Configuration.ManagementPackRelationship[]

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS

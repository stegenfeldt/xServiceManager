function Get-xSCSMEnumeration {
    [CmdletBinding()]
    param (
        # Search by Displayname
        [Parameter(ParameterSetName = 'ByDisplayName')]
        [string]
        $DisplayName,
        # Search by Class Name
        [Parameter(ParameterSetName = 'ByName')]
        [string]
        $Name,
        # Return ALL enums, beware of this one
        [Parameter(ParameterSetName = 'ReturnAll')]
        [switch]
        $All
    )
    
    begin {
        if (Get-SCManagementGroupConnection) {
            $mg = [Microsoft.EnterpriseManagement.EnterpriseManagementGroup]::new((Get-SCManagementGroupConnection)[0].ManagementServerName)
            $enums = $mg.EntityTypes.GetEnumerations()
        } else {
            throw "No SCSM Management Group connection in session.`nPlease connect to a Management Group using New-SCManagementGroupConnection"
        }
    }
    
    process {
        if ($PSCmdlet.ParameterSetName -eq 'ByDisplayName') {
            $matchedEnums = $enums | Where-Object { $_.DisplayName -like $DisplayName } 
        } elseif ($PSCmdlet.ParameterSetName -eq 'ByName') {
            $matchedEnums = $enums | Where-Object { $_.Name -like $Name } 
        } elseif ($PSCmdlet.ParameterSetName -eq 'ReturnAll') {
            if ($All) {
                $matchedEnums = $enums
            }
        } else {
            $matchedEnums = $null
        }
    }
    
    end {
        return $matchedEnums
    }
}
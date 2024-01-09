function Get-xSCSMRelatedObjectScript {
    [CmdletBinding()]
    param (
        # Get related objects by from source object
        [Parameter(ParameterSetName = 'BySource')]
        [Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject]
        $SourceObject,
        # Get related objects by from target object
        [Parameter(ParameterSetName = 'ByTarget')]
        [Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject]
        $TargetObject,
        # Filter on relationship class
        [Parameter(ParameterSetName = 'BySource')]
        [Parameter(ParameterSetName = 'ByTarget')]
        $RelationShipClass
    )
    
    begin {
        $traversalDepth = @{
            OneLevel  = [Microsoft.EnterpriseManagement.Common.TraversalDepth]::OneLevel
            Recursive = [Microsoft.EnterpriseManagement.Common.TraversalDepth]::Recursive
        }
        $defaultQueryOption = [Microsoft.EnterpriseManagement.Common.ObjectQueryOptions]::Default
        switch ($PSCmdlet.ParameterSetName) {
            'BySource' { 
                $smObject = $SourceObject
                $mg = $SourceObject.ManagementGroup
                #related objects (where source) method type
                [Type[]]$RelatedMethodType = @([Guid], [Microsoft.EnterpriseManagement.Common.TraversalDepth], [Microsoft.EnterpriseManagement.Common.ObjectQueryOptions])
                $RelatedByMethod = $mg.EntityObjects.GetType().GetMethod('GetRelationshipObjectsWhereSource', $RelatedMethodType)
                $RelatedByGenericMethod = $RelatedByMethod.MakeGenericMethod([Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject])
            }
            'ByTarget' {
                $smObject = $TargetObject
                $mg = $TargetObject.ManagementGroup
                #related objects (where target) method type
                [Type[]]$RelatedMethodType = @([Guid], [Microsoft.EnterpriseManagement.Common.ObjectQueryOptions])
                $RelatedByMethod = $mg.EntityObjects.GetType().GetMethod('GetRelationshipObjectsWhereTarget', $RelatedMethodType)
                $RelatedByGenericMethod = $RelatedByMethod.MakeGenericMethod([Microsoft.EnterpriseManagement.Common.EnterpriseManagementObject])
            }
            Default {}
        }
  
    }
    
    process {
        switch ($PSCmdlet.ParameterSetName) {
            'BySource' {
                $relationshipObjects = $RelatedByGenericMethod.Invoke($mg.EntityObjects, ([guid]$smObject.Id, $traversalDepth.OneLevel, $defaultQueryOption))
            }
            'ByTarget' {
                $relationshipObjects = $RelatedByGenericMethod.Invoke($mg.EntityObjects, ([guid]$smObject.Id, $defaultQueryOption))
            }
        }
    }
    
    end {
        return $relationshipObjects
    }
}
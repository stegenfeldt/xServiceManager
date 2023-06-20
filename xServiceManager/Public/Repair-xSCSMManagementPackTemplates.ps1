<#
This function will make sure that each activity in an SCSM workitem template has an Id with a prefix.
It will read a SCSM management pack xml-file and add the missing Ids in the templates found in the file.
The function will also add a prefix to the Ids if they are missing.
#>
function Repair-xSCSMManagementPackTemplates.ps1 {
    [CmdletBinding()]
    param (
        # Path to an exported management pack xml file
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    begin {
        # validate the path to the xml-file
        if (-not (Test-Path $Path)) {
            throw 'The path to the xml-file is not valid.'
        }

        # load the xml-file
        $xml = [xml](Get-Content $Path -Encoding UTF8)

        # verify that the xml-file is a SCSM management pack
        if ($null -eq $xml.ManagementPack) {
            throw 'The xml-file is not a SCSM management pack.'
        }

        # set dirty-flag to false
        $dirty = $false
    }

    process {
        # get all the templates in the xml-file
        $templates = $xml.ManagementPack.Templates.ObjectTemplate 

        # loop through all the templates
        foreach ($template in $templates) {
            # verify that the template is a SCSM workitem template
            if ($template.TypeId -eq 'ChangeManagement!System.WorkItem.ChangeRequestProjection') {
                # activites in a template are stored as <Object /> elements
                # get all the activities in the template
                $activities = $template.Object

                #loop through all the activities
                foreach ($activity in $activities) {
                    # the TypeConstraint attribute is in the Path attribute
                    # the Path attribute is formatted as $Context/Path/[Relationship='RelationshipAlias!RelationshipType' TypeConstraint='Alias!Type']$
                    # get the Type from the Path attribute using regex
                    $type = $activity.Path -replace '.*TypeConstraint=''.*!([^!]+)''.*', '$1'
                    # get the Alias from the Path attribute using regex
                    $alias = $activity.Path -replace '.*TypeConstraint=''([^!]+).*', '$1'

                    $prefix = ''

                    # System.WorkItem.Activity.ReviewActivity are Review Activities and have prefix RA
                    # System.WorkItem.Activity.ManualActivity are Manual Activities and have prefix MA
                    # System.WorkItem.Activity.ParallelActivity are Parallel Activities and have prefix PA
                    # System.WorkItem.Activity.SequentialActivity are Sequential Activities and have prefix SA
                    # Microsoft.SystemCenter.Orchestrator.RunbookAutomationActivity are Orchestrator Activities and have prefix RB
                    # RG.WorkItem.Extensions.PauseActivity are Pause Activities and have prefix WA
                    # set the prefix based on the type of activity
                    switch ($type) {
                        'System.WorkItem.Activity.ReviewActivity' {
                            $prefix = 'RA'
                            break
                        }
                        'System.WorkItem.Activity.ManualActivity' {
                            $prefix = 'MA'
                            break
                        }
                        'System.WorkItem.Activity.ParallelActivity' {
                            $prefix = 'PA'
                            break
                        }
                        'System.WorkItem.Activity.SequentialActivity' {
                            $prefix = 'SA'
                            break
                        }
                        'Microsoft.SystemCenter.Orchestrator.RunbookAutomationActivity' {
                            $prefix = 'RB'
                            break
                        }
                        'RG.WorkItem.Extensions.PauseActivity' {
                            $prefix = 'WA'
                            break
                        }
                        default {
                            # if the type of activity is not one of the above, write the Type to the output and continue
                            Write-Output "Unknown activity type: $type"						
                            continue
                        }
                    }

                    # find chile elements of the activity of type Property
                    # return the Property element with a Path attribute of $Context/Property[Type='CustomSystem_WorkItem_Library!System.WorkItem']/Id$
                    $idProperty = $activity.ChildNodes | Where-Object { $_.Name -eq 'Property' -and $_.Path -eq '$Context/Property[Type=''CustomSystem_WorkItem_Library!System.WorkItem'']/Id$' }

                    # if the Id attribute is missing, then add it
                    # a missing Id is indicated by a missing Property element with a Path attribute of $Context/Property[Type='CustomSystem_WorkItem_Library!System.WorkItem']/Id$
                    if ($null -eq $idProperty) {
                        $property = $activity.AppendChild($xml.CreateElement('Property'))
                        $property.SetAttribute('Path', '$Context/Property[Type=''CustomSystem_WorkItem_Library!System.WorkItem'']/Id$')
                        # set the Property value to the prefix
                        $property.InnerText = $prefix + '{0}'

                        #set the dirty-flag to true
                        $dirty = $true
                    }
                }
            }
        }

        # if the dirty-flag is true, then save the xml-file
        if ($dirty) {
            # increment the revision number in the ManagementPack.Manifest.Identity.Version element
            $version = $xml.ManagementPack.Manifest.Identity.Version -as [version]
            $version = $version.Major, $version.Minor, $version.Build, ($version.Revision + 1) -join '.'

            $xml.ManagementPack.Manifest.Identity.Version = $version.ToString()
            # save the xml-file
            $xml.Save($Path)
        }
    }
}
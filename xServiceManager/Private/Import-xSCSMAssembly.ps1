function Import-xSCSMAssembly {
	[CmdletBinding()]
	param (
		# Assembly (DLL) to load into memory
		[Parameter(Mandatory = $true, Position = 0)]
		[System.IO.FileInfo]$AssemblyFile,
		# Load with partial name
		[Parameter(Mandatory = $false, Position = 1)]
		[switch]$LoadWithPartialName
	)
	
	begin {
		# Verify paths
		$validPath = Test-Path $AssemblyFile.FullName
	}
	
	process {
		$Error.Clear()
		if ($validPath) {
			if ($LoadWithPartialName) {
				[Reflection.Assembly]::LoadWithPartialName($AssemblyFile)
			} else {
				[Reflection.Assembly]::LoadFile($AssemblyFile)
			}
		}
	}
	
	end {
		if ($Error.Count -gt 0) {
			return $Error
		} else {
			return $true
		}
	}
}
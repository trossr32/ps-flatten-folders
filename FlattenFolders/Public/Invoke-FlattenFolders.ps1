function Invoke-FlattenFolders {
    <#
    .SYNOPSIS
    Moves files from all sub-directories to the parent directory and optionally delete sub-directories.

    .DESCRIPTION
    Moves files from all sub-directories to the parent directory. If files with duplicate names are found then their file name
    will have a guid appended to make them unique.

    Unless the Force parameter is used there will be a prompt for confirmation before both the renaming of any files (if required)
    and the moving of any files.

    Can be run against: 
    
    > a single directory
    > a collection of directories piped into the module.

    .PARAMETER Directory
    Optional. The parent directory where files from all sub-directories will be moved. If neither this nor the Directories
    parameter are set then the current location will be used.
    
    .PARAMETER Directories
    Optional. A collection of parent directories where files from all sub-directories will be moved. If neither this nor 
    the Directory parameter are set then the current location will be used.
    
    .PARAMETER Force
    Optional. If supplied this bypasses the confirmation prompt before both renaming and moving files.
    
    .PARAMETER DeleteSubDirectories
    Optional. If supplied all subdirectories will be deleted once all files have been moved.
    
    .EXAMPLE
    All files in all sub-directories in the current location (C:\) will be moved to the current location (C:\) with a 
    confirmation prompt before moving:
    PS C:\> Invoke-FlattenFolder
    
    .EXAMPLE
    All files in all sub-directories in C:\Videos\ will be moved to C:\Videos\ without a confirmation prompt:
    PS C:\> Invoke-FlattenFolder -Directory "C:\Videos" -Force
    
    .EXAMPLE
    All files in all sub-directories in C:\Videos\ will be moved to C:\Videos\ without a confirmation prompt and all
    sub-directories will be deleted once the files have been moved:
    PS C:\> Invoke-FlattenFolder -Directory "C:\Videos" -Force -DeleteSubDirectories
    
    .EXAMPLE
    All files in all sub-directories in the piped array of directories (C:\Videos\ and C:\Music\) will be moved to their 
    respective parents with a confirmation prompt before moving:
    PS C:\> "C:\Videos\","C:\Music\" | Invoke-FlattenFolder
    #>

    [CmdletBinding()]

    Param(
        [Parameter(Mandatory=$false, Position=0)]
        [Alias("D")]
        [String]$Directory,

        [Parameter(Mandatory=$false, ValueFromPipeline=$true)]
        [String[]]$Directories,

        [Parameter(Mandatory=$false)]
        [Alias("F")]
        [Switch]$Force,

        [Parameter(Mandatory=$false)]
        [Alias("DS")]
        [Switch]$DeleteSubDirectories
    )

    Begin {
        $dirs = @()
        $isValid = $true
    }

    Process {
        # Check if a directory list was supplied
        if ($PSBoundParameters.ContainsKey('Directories')) {
            # validate directory exists
            if (-Not (Test-Path -Path $Directories)) {
                $isValid = $false

                Write-Error("One or more of the supplied directories does not exist.")

                return
            }
            
            $dirs += $Directories

            return
        }

        # Check if a file was supplied
        if ($PSBoundParameters.ContainsKey('Directory')) {
            if (Test-Path -Path $Directory) {
                $dirs += $Directory

                return
            }
            
            $isValid = $false

            Write-Error("The supplied directory does not exist.")

            return
        }

        #$dirs += (Get-Location).Path
    }

    End {
        if (-Not $isValid) {
            return
        }

        $subDirs = @() 
        $files = @()
        
        foreach ($dir in $dirs) {
            # enumerate subdirectories
            Get-ChildItem $dir -Recurse -Directory | Select-Object FullName | Foreach-Object {
                $subdirs += $_.FullName
            }

            # enumerate files
            Get-ChildItem $dir -Recurse -File | Group-Object FullName | Select-Object Name | ForEach-Object {
                $files += $_.Name
            }
        }

        # check we've found some files
        if ($files.Length -eq 0) {
            Write-Host "No files found"

            return
        }

        # check to see if we'll have any naming conflicts
        $duplicates = @()

        foreach ($dir in $dirs) {
            $duplicates += (Get-DuplicateFiles -dir $dir)
        }

        # if we have naming conflicts and the Force parameter has not been passed prompt the user to confirm file renaming
        if (($duplicates.Length -gt 0) -and (-Not $PSBoundParameters.ContainsKey('Force'))) {
            # the total number of duplicates is the number reported plus the count of unique instances within those duplicates
            # one instance from each group is not returned from the Compare-Object call
            $cDuplicates = ($duplicates | Select-Object -Unique).Length + $duplicates.Length
            
            $header = "$cDuplicates files with the same name were found. These files will have a guid appended to the file name to make them unique."
            $question = "Are you happy to continue?"

            if (-Not (Get-YesNoAsBool($header, $question))) {
                return
            }
        }
        
        # if the Force parameter has not been passed calculate affected files and prompt the user to confirm the file move
        if (-Not $PSBoundParameters.ContainsKey('Force')) {
            # ask for confirmation
            $cDirs = $dirs.Length
            $cSubDirs = $subDirs.Length
            $cFiles = $files.Length
            $nDir = $cDirs -gt 1 ? "directories" : "directory"
            $nSub = $PSBoundParameters.ContainsKey('DeleteSubDirectories') ? "and delete all sub-directories" : ""

            $header = "You are about to move $cFiles files from $cSubDirs sub-directories into $cDirs parent $nDir $nSub"
            $question = "Are you sure you want to continue?"

            if (-Not (Get-YesNoAsBool($header, $question))) {
                return
            }
        }

        $i = 0

        foreach ($dir in $dirs) {
            # enumerate files
            Write-Progress -Activity "Moving file $($i + 1) of $($files.Length)" -PercentComplete ((($i + 1) / $files.Length) * 100)

            # get just the duplicates in this directory
            $duplicates = (Get-DuplicateFiles -dir $dir)

            Get-ChildItem $dir -Recurse -File | Group-Object FullName | Select-Object Name | ForEach-Object {
                if ($duplicates -contains (Split-Path $_.Name -leaf)) {
                    $newName = ((Split-Path $_.Name -LeafBase) + "_" + [GUID]::NewGuid().ToString('D') + (Split-Path $_.Name -Extension))

                    Rename-Item -Path $_.Name -NewName $newName

                    Move-Item -Path (Join-Path -Path (Split-Path $_.Name) -ChildPath $newName) -Destination $dir
                } else {
                    Move-Item -Path $_.Name -Destination $dir
                }
            }

            # delete sub-directories if requested
            if ($PSBoundParameters.ContainsKey('DeleteSubDirectories')) {
                Get-ChildItem $dir -Directory | ForEach-Object { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
            }
        }
    }
}
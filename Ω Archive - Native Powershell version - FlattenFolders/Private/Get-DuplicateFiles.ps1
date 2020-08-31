Function Get-DuplicateFiles($dir) {
    <#
    .DESCRIPTION
    Finds any duplicate files cross all sub-directories in the supplied directory.

    .PARAMETER dir
    The directory to find duplicate files in.

    .OUTPUTS
    An array of duplicate file names, or an empty array if no duplicates were found.
    #>

    $duplicates = @()
    $filenames = Get-ChildItem $dir -Recurse -File | Group-Object FullName | Select-Object Name | ForEach-Object {Split-Path $_.Name -leaf}
    $uniqueFilenames = $filenames | Select-Object -Unique
    
    if ($null -ne $uniqueFilenames) {
        $duplicates += Compare-Object -ReferenceObject $uniqueFilenames -DifferenceObject $filenames -PassThru
    }

    return $duplicates
}
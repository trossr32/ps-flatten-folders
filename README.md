# FlattenFolders
A Powershell module that moves files from all sub-directories to the parent directory.

Available in the [Powershell Gallery](https://www.powershellgallery.com/packages/FlattenFolders)

## Description
Moves files from all sub-directories to the parent directory. If files with duplicate names are found then their file name
will have a guid appended to make them unique.

Unless the Force parameter is used there will be a prompt for confirmation before both the renaming of any files (if required)
and the moving of any files.

Can be run against: 

* a single directory
* a collection of directories piped into the module.

## Installation

```powershell
Install-Module FlattenFolders
```

## Parameters

#### -Directory (alias -D)
*Optional*. The parent directory where files from all sub-directories will be moved. If neither this nor the Directories
parameter are set then the current location will be used.

#### -Directories
*Optional*. A collection of parent directories where files from all sub-directories will be moved. If neither this nor 
the Directory parameter are set then the current location will be used.

#### -Force (alias -F)
*Optional*. If supplied this bypasses the confirmation prompt before both renaming and moving files.

#### -DeleteSubDirectories (alias -DS)
*Optional*. If supplied all subdirectories will be deleted once all files have been moved.

## Examples

Given the following directory structure:

📦flatten me
 ┣ 📂parent a
 ┃ ┣ 📂sub a
 ┃ ┃ ┣ 📜a a New Text Document - Copy (2).txt
 ┃ ┃ ┣ 📜a a New Text Document - Copy.txt
 ┃ ┃ ┗ 📜a a New Text Document.txt
 ┃ ┣ 📂sub b
 ┃ ┃ ┣ 📜a b New Text Document - Copy (2).txt
 ┃ ┃ ┣ 📜a b New Text Document - Copy.txt
 ┃ ┃ ┗ 📜a b New Text Document.txt
 ┃ ┣ 📂sub c
 ┃ ┃ ┣ 📜a c New Text Document - Copy (2).txt
 ┃ ┃ ┣ 📜a c New Text Document - Copy.txt
 ┃ ┃ ┗ 📜a c New Text Document.txt
 ┃ ┗ 📂sub d
 ┃ ┃ ┣ 📜a c New Text Document - Copy.txt
 ┃ ┃ ┣ 📜New Text Document - Copy (2).txt
 ┃ ┃ ┗ 📜New Text Document.txt
 ┣ 📂parent b
 ┃ ┣ 📂sub a
 ┃ ┃ ┣ 📜New Text Document - Copy (2).txt
 ┃ ┃ ┣ 📜New Text Document - Copy.txt
 ┃ ┃ ┗ 📜New Text Document.txt
 ┃ ┣ 📂sub b
 ┃ ┃ ┣ 📜b New Text Document - Copy (2).txt
 ┃ ┃ ┣ 📜b New Text Document - Copy.txt
 ┃ ┃ ┗ 📜b New Text Document.txt
 ┃ ┣ 📂sub c
 ┃ ┃ ┣ 📜c New Text Document - Copy (2).txt
 ┃ ┃ ┣ 📜c New Text Document - Copy.txt
 ┃ ┃ ┗ 📜c New Text Document.txt
 ┃ ┗ 📂sub d
 ┃ ┃ ┣ 📜c New Text Document - Copy.txt
 ┃ ┃ ┣ 📜New Text Document - Copy (2).txt
 ┃ ┃ ┗ 📜New Text Document.txt
 ┗ 📂parent c

Ruuning the following command...

```powershell
PS C:\>@("C:\temp\flatten me\parent a","C:\temp\flatten me\parent b","C:\temp\flatten me\parent c") | Invoke-FlattenFolders -Force -DeleteSubDirectories
```

...will move all files from each parent directory's sub-directories into the parent directory and then delete the sub-directories. Files with duplicate names will have a Guid appended to their file names. The result will look like this:

📦flatten me
 ┣ 📂parent a
 ┃ ┣ 📜a a New Text Document - Copy (2).txt
 ┃ ┣ 📜a a New Text Document - Copy.txt
 ┃ ┣ 📜a a New Text Document.txt
 ┃ ┣ 📜a b New Text Document - Copy (2).txt
 ┃ ┣ 📜a b New Text Document - Copy.txt
 ┃ ┣ 📜a b New Text Document.txt
 ┃ ┣ 📜a c New Text Document - Copy (2).txt
 ┃ ┣ 📜a c New Text Document - Copy_58888d2c-b089-472b-b166-742701456252.txt
 ┃ ┣ 📜a c New Text Document - Copy_59612b2e-d42b-474d-b522-1ffdc4e302fb.txt
 ┃ ┣ 📜a c New Text Document.txt
 ┃ ┣ 📜New Text Document - Copy (2).txt
 ┃ ┗ 📜New Text Document.txt
 ┣ 📂parent b
 ┃ ┣ 📜b New Text Document - Copy (2).txt
 ┃ ┣ 📜b New Text Document - Copy.txt
 ┃ ┣ 📜b New Text Document.txt
 ┃ ┣ 📜c New Text Document - Copy (2).txt
 ┃ ┣ 📜c New Text Document - Copy_1e57ea51-bc54-4f44-a1db-98257b4e839b.txt
 ┃ ┣ 📜c New Text Document - Copy_eb5b533c-19b5-4898-9c60-5edc6e6d7ceb.txt
 ┃ ┣ 📜c New Text Document.txt
 ┃ ┣ 📜New Text Document - Copy (2)_2a39376b-7b8b-4087-bfc4-7c0f25cfc96e.txt
 ┃ ┣ 📜New Text Document - Copy (2)_6ab8a5a8-a7b7-4e2b-8eb2-c6e64d7458ea.txt
 ┃ ┣ 📜New Text Document - Copy.txt
 ┃ ┣ 📜New Text Document_0495b454-56d7-41a6-9f6c-f4ce39a35c3a.txt
 ┃ ┗ 📜New Text Document_79c2dd84-b1bf-4660-ba10-3229848b867f.txt
 ┗ 📂parent c

All files in all sub-directories in the current location (C:\) will be moved to the current location (C:\) with a 
confirmation prompt before moving:

```powershell
PS C:\> Invoke-FlattenFolder
```

All files in all sub-directories in C:\Videos\ will be moved to C:\Videos\ without a confirmation prompt:

```powershell
PS C:\> Invoke-FlattenFolder -Directory "C:\Videos" -Force
```

All files in all sub-directories in C:\Videos\ will be moved to C:\Videos\ without a confirmation prompt and all
sub-directories will be deleted once the files have been moved:

```powershell
PS C:\> Invoke-FlattenFolder -Directory "C:\Videos" -Force -DeleteSubDirectories
```

All files in all sub-directories in the piped array of directories (C:\Videos\ and C:\Music\) will be moved to their 
respective parents with a confirmation prompt before moving:

```powershell
PS C:\> "C:\Videos\","C:\Music\" | Invoke-FlattenFolder
```
# FlattenFolders

[![PowerShell Gallery Version](https://img.shields.io/powershellgallery/v/FlattenFolders?label=FlattenFolders&logo=powershell&style=plastic)](https://www.powershellgallery.com/packages/FlattenFolders)
[![PowerShell Gallery](https://img.shields.io/powershellgallery/dt/FlattenFolders?style=plastic)](https://www.powershellgallery.com/packages/FlattenFolders)

A Powershell module that moves files from all sub-directories to the parent directory.

Available in the [Powershell Gallery](https://www.powershellgallery.com/packages/FlattenFolders)

## Description
Moves files from all sub-directories to the parent directory. If files with duplicate names are found then their file name will have a guid appended to make them unique.

Supports WhatIf. If supplied this will output a formatted table of the from and to file locations that will result from running the cmdlet.

Can be run against: 

* a single directory
* a collection of directories piped into the module.

## Installation (from the Powershell Gallery)

```powershell
Install-Module FlattenFolders
Import-Module FlattenFolders
```

## Parameters

#### -Directory (alias -D)
*Optional*. The parent directory where files from all sub-directories will be moved. If neither this nor the Directories parameter are set then the current location will be used.

#### -Directories
*Optional*. A collection of parent directories where files from all sub-directories will be moved. If neither this nor the Directory parameter are set then the current location will be used.

#### -WhatIf (alias -F)
*Optional*. If supplied this will output a formatted table of the from and to file locations that will result from running the cmdlet.

#### -DeleteSubDirectories (alias -DS)
*Optional*. If supplied all sub-directories will be deleted once all files have been moved.

## Example

Given the following directory structure:

📦flatten me<br />
 ┣ 📂parent a<br />
 ┃ ┣ 📂sub a<br />
 ┃ ┃ ┣ 📜a a New Text Document - Copy (2).txt<br />
 ┃ ┃ ┣ 📜a a New Text Document - Copy.txt<br />
 ┃ ┃ ┗ 📜a a New Text Document.txt<br />
 ┃ ┣ 📂sub b<br />
 ┃ ┃ ┣ 📜a b New Text Document - Copy (2).txt<br />
 ┃ ┃ ┣ 📜a b New Text Document - Copy.txt<br />
 ┃ ┃ ┗ 📜a b New Text Document.txt<br />
 ┃ ┣ 📂sub c<br />
 ┃ ┃ ┣ 📜a c New Text Document - Copy (2).txt<br />
 ┃ ┃ ┣ 📜a c New Text Document - Copy.txt<br />
 ┃ ┃ ┗ 📜a c New Text Document.txt<br />
 ┃ ┗ 📂sub d<br />
 ┃ ┃ ┣ 📜a c New Text Document - Copy.txt<br />
 ┃ ┃ ┣ 📜New Text Document - Copy (2).txt<br />
 ┃ ┃ ┗ 📜New Text Document.txt<br />
 ┣ 📂parent b<br />
 ┃ ┣ 📂sub a<br />
 ┃ ┃ ┣ 📜New Text Document - Copy (2).txt<br />
 ┃ ┃ ┣ 📜New Text Document - Copy.txt<br />
 ┃ ┃ ┗ 📜New Text Document.txt<br />
 ┃ ┣ 📂sub b<br />
 ┃ ┃ ┣ 📜b New Text Document - Copy (2).txt<br />
 ┃ ┃ ┣ 📜b New Text Document - Copy.txt<br />
 ┃ ┃ ┗ 📜b New Text Document.txt<br />
 ┃ ┣ 📂sub c<br />
 ┃ ┃ ┣ 📜c New Text Document - Copy (2).txt<br />
 ┃ ┃ ┣ 📜c New Text Document - Copy.txt<br />
 ┃ ┃ ┗ 📜c New Text Document.txt<br />
 ┃ ┗ 📂sub d<br />
 ┃ ┃ ┣ 📜c New Text Document - Copy.txt<br />
 ┃ ┃ ┣ 📜New Text Document - Copy (2).txt<br />
 ┃ ┃ ┗ 📜New Text Document.txt<br />
 ┗ 📂parent c

Running the following command...

```powershell
PS C:\>@("C:\temp\flatten me\parent a","C:\temp\flatten me\parent b","C:\temp\flatten me\parent c") | Invoke-FlattenFolders -DeleteSubDirectories
```

...will move all files from each parent directory's sub-directories into the parent directory and then delete the sub-directories. Files with duplicate names will have a Guid appended to their file names. The result will look like this:

📦flatten me<br />
 ┣ 📂parent a<br />
 ┃ ┣ 📜a a New Text Document - Copy (2).txt<br />
 ┃ ┣ 📜a a New Text Document - Copy.txt<br />
 ┃ ┣ 📜a a New Text Document.txt<br />
 ┃ ┣ 📜a b New Text Document - Copy (2).txt<br />
 ┃ ┣ 📜a b New Text Document - Copy.txt<br />
 ┃ ┣ 📜a b New Text Document.txt<br />
 ┃ ┣ 📜a c New Text Document - Copy (2).txt<br />
 ┃ ┣ 📜a c New Text Document - Copy_58888d2c-b089-472b-b166-742701456252.txt<br />
 ┃ ┣ 📜a c New Text Document - Copy_59612b2e-d42b-474d-b522-1ffdc4e302fb.txt<br />
 ┃ ┣ 📜a c New Text Document.txt<br />
 ┃ ┣ 📜New Text Document - Copy (2).txt<br />
 ┃ ┗ 📜New Text Document.txt<br />
 ┣ 📂parent b<br />
 ┃ ┣ 📜b New Text Document - Copy (2).txt<br />
 ┃ ┣ 📜b New Text Document - Copy.txt<br />
 ┃ ┣ 📜b New Text Document.txt<br />
 ┃ ┣ 📜c New Text Document - Copy (2).txt<br />
 ┃ ┣ 📜c New Text Document - Copy_1e57ea51-bc54-4f44-a1db-98257b4e839b.txt<br />
 ┃ ┣ 📜c New Text Document - Copy_eb5b533c-19b5-4898-9c60-5edc6e6d7ceb.txt<br />
 ┃ ┣ 📜c New Text Document.txt<br />
 ┃ ┣ 📜New Text Document - Copy (2)_2a39376b-7b8b-4087-bfc4-7c0f25cfc96e.txt<br />
 ┃ ┣ 📜New Text Document - Copy (2)_6ab8a5a8-a7b7-4e2b-8eb2-c6e64d7458ea.txt<br />
 ┃ ┣ 📜New Text Document - Copy.txt<br />
 ┃ ┣ 📜New Text Document_0495b454-56d7-41a6-9f6c-f4ce39a35c3a.txt<br />
 ┃ ┗ 📜New Text Document_79c2dd84-b1bf-4660-ba10-3229848b867f.txt<br />
 ┗ 📂parent c
 
 ## Further examples

All files in all sub-directories in the current location (C:\) will be moved to the current location (C:\):

```powershell
PS C:\> Invoke-FlattenFolder
```

Displays an output table to terminal detailing that all files in all sub-directories in C:\Videos\ would be moved to C:\Videos\:

```powershell
PS C:\> Invoke-FlattenFolder -Directory "C:\Videos" -WhatIf
```

All files in all sub-directories in C:\Videos\ will be moved to C:\Videos\ and all sub-directories will be deleted once the files have been moved:

```powershell
PS C:\> Invoke-FlattenFolder -Directory "C:\Videos" -DeleteSubDirectories
```

All files in all sub-directories in the piped array of directories (C:\Videos\ and C:\Music\) will be moved to their respective parents:

```powershell
PS C:\> "C:\Videos\","C:\Music\" | Invoke-FlattenFolder
```

## Building the module and importing locally

### Build the .NET core solution

```powershell
dotnet build [Github clone/download directory]\ps-flatten-folders\src\PsFlattenFoldersCmdlet.sln
```

### Copy the built files to your Powershell modules directory

Remove any existing installation in this directory, create a new module directory and copy all the built files.

```powershell
Remove-Item "C:\Users\[User]\Documents\PowerShell\Modules\FlattenFolders" -Recurse -Force -ErrorAction SilentlyContinue
New-Item -Path 'C:\Users\[User]\Documents\PowerShell\Modules\FlattenFolders' -ItemType Directory
Get-ChildItem -Path "[Github clone/download directory]\ps-flatten-folders\src\PsFlattenFoldersCmdlet\bin\Debug\netcoreapp3.1\" | Copy-Item -Destination "C:\Users\[User]\Documents\PowerShell\Modules\FlattenFolders" -Recurse
```

### Import the module to your session

```powershell
Import-Module "C:\Users\[User]\Documents\PowerShell\Modules\FlattenFolders\FlattenFolders.dll"
```

## Notes

Initially this module was written in native Powershell but has since been upgraded to a .NET core 3.1 Cmdlet. I've archived the Powershell version in case anyone is interested in viewing the differences between the implementations.

- The .NET core version of the module is in the [src](https://github.com/trossr32/ps-flatten-folders/tree/master/src) directory.
- The Native Powershell version is in the [archived native PS version](https://github.com/trossr32/ps-flatten-folders/tree/master/archived%20native%20PS%20version) directory.

## Contribute

Please raise an issue if you find a bug or want to request a new feature, or create a pull request to contribute.

<a href='https://ko-fi.com/K3K22CEIT' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://cdn.ko-fi.com/cdn/kofi4.png?v=2' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

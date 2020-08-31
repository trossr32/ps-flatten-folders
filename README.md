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

## Installation (from the Poweshell Gallery)

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
PS C:\>@("C:\temp\flatten me\parent a","C:\temp\flatten me\parent b","C:\temp\flatten me\parent c") | Invoke-FlattenFolders -Force -DeleteSubDirectories
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

Initially this module was written in native Powershell but has since been upgraded to a .NET core 3.1 Cmdlet. I've archived the Powershell version in case anyone is interested in viewing teh differences between the 2 implementations.

- The .NET core version of the module is in the (https://github.com/trossr32/ps-flatten-folders/tree/master/src)[src] directory.
- The Native Powershell version is in the (https://github.com/trossr32/ps-flatten-folders/tree/master/Ω Archive - Native Powershell version - FlattenFolders)[Ω Archive - Native Powershell version - FlattenFolders] directory.
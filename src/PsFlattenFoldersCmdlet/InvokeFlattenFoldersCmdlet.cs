using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using FlattenFolders.Models;

[assembly: InternalsVisibleTo("PsFlattenFoldersCmdlet.Tests")]
namespace FlattenFolders;

/// <summary>
/// <para type="synopsis">Moves files from all sub-directories to the parent directory and optionally delete sub-directories.</para>
/// <para type="description">
/// Moves files from all sub-directories to the parent directory.If files with duplicate names are found then their file name will have a GUID appended to make them unique.
/// </para>
/// <para type="description">
/// Supports WhatIf. If supplied this will output a formatted table of the from and to file locations that will result from running the cmdlet.
/// </para>
/// <para type="description">
/// Can be run against:
/// </para>
/// <para type="description">
/// > a single directory
/// </para>
/// <para type="description">
/// > a collection of directories piped into the module.
/// </para>
/// <example>
///     <para>All files in all sub-directories in the current location (C:\) will be moved to the current location(C:\):</para>
///     <code>PS C:\> Invoke-FlattenFolders</code>
/// </example>
/// <example>
///     <para>Displays an output table to terminal detailing that all files in all sub-directories in C:\Videos\ would be moved to C:\Videos\:</para>
///     <code>PS C:\> Invoke-FlattenFolders -Directory "C:\Videos" -WhatIf</code>
/// </example>
/// <example>
///     <para>All files in all sub-directories in C:\Videos\ will be moved to C:\Videos\ and all sub-directories will be deleted once the files have been moved:</para>
///     <code>PS C:\> Invoke-FlattenFolders -Directory "C:\Videos" -DeleteSubDirectories</code>
/// </example>
/// <example>
///     <para>All files in all sub-directories in the piped array of directories(C:\Videos\ and C:\Music\) will be moved to their respective parents:</para>
///     <code>PS C:\> "C:\Videos\","C:\Music\" | Invoke-FlattenFolders</code>
/// </example>
/// <para type="link" uri="(https://github.com/trossr32/ps-flatten-folders)">[Github]</para>
/// </summary>
[Cmdlet(VerbsLifecycle.Invoke, "FlattenFolders", HelpUri = "https://github.com/trossr32/ps-flatten-folders")]
public class InvokeFlattenFoldersCmdlet : PSCmdlet
{
    #region Parameters

    /// <summary>
    /// <para type="description">
    /// The parent directory where files from all sub-directories will be moved.
    /// If neither this nor the Directories parameter are set then the current location will be used.
    /// </para>
    /// </summary>
    [Parameter(Mandatory = false, Position = 0)]
    [Alias("D")]
    public string Directory { get; set; }

    /// <summary>
    /// <para type="description">
    /// A collection of parent directories where files from all sub-directories will be moved.
    /// If neither this nor the Directory parameter are set then the current location will be used.
    /// </para>
    /// </summary>
    [Parameter(Mandatory = false, ValueFromPipeline = true)]
    public List<string> Directories { get; set; }

    /// <summary>
    /// <para type="description">
    /// If supplied this will output a formatted table of the from and to file locations that will result from running the cmdlet.
    /// </para>
    /// </summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter WhatIf { get; set; }

    /// <summary>
    /// <para type="description">
    /// If supplied all subdirectories will be deleted once all files have been moved.
    /// </para>
    /// </summary>
    [Parameter(Mandatory = false)]
    [Alias("DS")]
    public SwitchParameter DeleteSubDirectories { get; set; }

    #endregion

    private bool _isValid;
    private FileProcessContainer _container;

    internal void ProcessInternal()
    {
        BeginProcessing();
        ProcessRecord();
        EndProcessing();
    }

    /// <summary>
    /// Implements the <see cref="BeginProcessing"/> method for <see cref="InvokeFlattenFoldersCmdlet"/>.
    /// Initialise temporary containers
    /// </summary>
    protected override void BeginProcessing()
    {
        _container = new FileProcessContainer();
        _isValid = true;
    }

    /// <summary>
    /// Implements the <see cref="ProcessRecord"/> method for <see cref="InvokeFlattenFoldersCmdlet"/>.
    /// Validates input directory/directories exist and builds a list of directories to process in the EndProcessing method.
    /// </summary>
    protected override void ProcessRecord()
    {
        // Check if a directory list was supplied
        if ((Directories ?? []).Count is not 0)
        {
            Directories.ForEach(d =>
            {
                if (System.IO.Directory.Exists(d))
                    return;

                _isValid = false;

                ThrowTerminatingError(new ErrorRecord(new Exception($"Directory not found: {d}, terminating."), null, ErrorCategory.InvalidArgument, null));
            });

            _container.SourceDirectories.AddRange(Directories);

            return;
        }

        // Check if a directory was supplied
        if (!string.IsNullOrWhiteSpace(Directory))
        {
            if (!System.IO.Directory.Exists(Directory))
            {
                _isValid = false;

                ThrowTerminatingError(new ErrorRecord(new Exception($"Directory not found: {Directory}, terminating."), null, ErrorCategory.InvalidArgument, null));
            }

            _container.SourceDirectories.Add(Directory);

            return;
        }

        _container.SourceDirectories.Add(SessionState.Path.CurrentFileSystemLocation.Path);
    }

    /// <summary>
    /// Implements the <see cref="EndProcessing"/> method for <see cref="InvokeFlattenFoldersCmdlet"/>.
    /// Perform the folder flattening on the configured directories.
    /// </summary>
    protected override void EndProcessing()
    {
        if (!_isValid)
            return;

        _container.SourceDirectories.ForEach(d =>
        {
            _container.Files.AddRange(System.IO.Directory.GetFiles(d, "*", SearchOption.AllDirectories)
                .ToList()
                .Select(f => new SourceFile(d, f, Path.GetFileName(f))));

            _container.SubDirectoryCount += System.IO.Directory.GetDirectories(d, "*", SearchOption.AllDirectories).Length;
        });

        if (_container.Files.Count is 0)
            ThrowTerminatingError(new ErrorRecord(new Exception("No files found, terminating."), null, ErrorCategory.InvalidArgument, null));

        // check for duplicate file names and build old to new file mappings
        _container.BuildDuplicatesAndFileMappings();

        // validate the user has the required permissions
        // TODO - assert this code works as expected
        //var isValid = ValidateMovePermissions();
        //if (!isValid)
        //    return;

        // if this is a 'what if' run then output the expected results
        if (WhatIf.IsPresent)
        {
            ProcessWhatIf();

            return;
        }

        // move all files to their associated parent directory
        foreach (var fileMapping in _container.FileMappings)
        {
            File.Move(fileMapping.OldFile, fileMapping.NewFile);
        }

        // if specified, delete all sub-directories
        if (DeleteSubDirectories)
            _container.SourceDirectories.ForEach(d =>
            {
                foreach (var sub in System.IO.Directory.GetDirectories(d))
                {
                    System.IO.Directory.Delete(sub, true);
                }
            });
    }

    /// <summary>
    /// A WhatIf switch parameter was passed so calculate what will happen and output a table to terminal
    /// </summary>
    private void ProcessWhatIf()
    {
        var filesText = $"{_container.Files.Count} file{(_container.Files.Count == 1 ? "" : "s")}";
        var subDirText = $"{_container.SubDirectoryCount} sub-director{(_container.SubDirectoryCount == 1 ? "y" : "ies")}";
        var dirText = $"{_container.SourceDirectories.Count} parent director{(_container.SourceDirectories.Count == 1 ? "y" : "ies")}{(DeleteSubDirectories ? " and all sub-directories would be deleted" : "")}";

        List<(int ix, string dir)> parentDirMappings =
        [
            .. _container.SourceDirectories
                .Select((d, i) => (ix: i, dir: d))
                .OrderBy(o => o.ix),
        ];

        // local function to replace file location in output with associated [Parent n] string
        string ReplaceParentDir(string file) => 
            parentDirMappings.Aggregate(file, (current, mapping) => current.Replace(mapping.dir.TrimEnd('/', '\\'), $"[Parent {mapping.ix}]"));

        List<string> output =
        [
            "",
            $"{filesText} would be moved from {subDirText} into {dirText}",
            "",
            "The following table shows file moves where:",
            ""
        ];

        output.AddRange(parentDirMappings.Select(m => $"[Parent {m.ix}] = {m.dir}"));

        var maxOldStringLength = _container.FileMappings.Max(f => ReplaceParentDir(f.OldFile).Length);
        var maxNewStringLength = _container.FileMappings.Max(f => ReplaceParentDir(f.NewFile).Length);

        output.AddRange(new List<string>
        {
            "",
            $"|={"".PadRight(maxOldStringLength, '=')}==={"".PadRight(maxNewStringLength, '=')}=|",
            $"| {"Old file".PadRight(maxOldStringLength, ' ')} | {"New file".PadRight(maxNewStringLength, ' ')} |",
            $"|-{"".PadRight(maxOldStringLength, '-')}---{"".PadRight(maxNewStringLength, '-')}-|"
        });

        output.AddRange(_container.FileMappings.Select(f => $"| {ReplaceParentDir(f.OldFile).PadRight(maxOldStringLength, ' ')} | {ReplaceParentDir(f.NewFile).PadRight(maxNewStringLength, ' ')} |"));

        output.Add($"|={"".PadRight(maxOldStringLength, '=')}==={"".PadRight(maxNewStringLength, '=')}=|");
        output.Add("");

        WriteObject(output, true);
    }

    /// <summary>
    /// Validates whether the user has write permissions to the file sin the supplied directories
    /// </summary>
    /// <returns></returns>
    private bool ValidateMovePermissions()
    {
#pragma warning disable SYSLIB0003
        FileIOPermission f = new(FileIOPermissionAccess.Write, _container.FileMappings.Select(fm => fm.OldFile).ToArray());
#pragma warning restore SYSLIB0003

        try
        {
            f.Demand();

            return true;
        }
        catch (Exception)
        {
            ThrowTerminatingError(new ErrorRecord(new Exception($"Insufficient permissions to move one or all of the files in the supplied director{(_container.SourceDirectories.Count == 1 ? "y" : "ies")}"), null, ErrorCategory.InvalidArgument, null));

            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PsFlattenFoldersCmdlet
{
    /// <summary>
    /// <para type="synopsis">Moves files from all sub-directories to the parent directory and optionally delete sub-directories.</para>
    /// <para type="description">
    /// Moves files from all sub-directories to the parent directory.If files with duplicate names are found then their file name will have a guid appended to make them unique.
    /// </para>
    /// <para type="description">
    /// Unless the Force parameter is used there will be a prompt for confirmation before both the renaming of any files (if required) and the moving of any files.
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
    ///     <para>All files in all sub-directories in the current location (C:\) will be moved to the current location(C:\) with a confirmation prompt before moving:</para>
    ///     <code>PS C:\> Invoke-FlattenFolder</code>
    /// </example>
    /// <example>
    ///     <para>All files in all sub-directories in C:\Videos\ will be moved to C:\Videos\ without a confirmation prompt:</para>
    ///     <code>PS C:\> Invoke-FlattenFolder -Directory "C:\Videos" -Force</code>
    /// </example>
    /// <example>
    ///     <para>All files in all sub-directories in C:\Videos\ will be moved to C:\Videos\ without a confirmation prompt and all sub-directories will be deleted once the files have been moved:</para>
    ///     <code>PS C:\> Invoke-FlattenFolder -Directory "C:\Videos" -Force -DeleteSubDirectories</code>
    /// </example>
    /// <example>
    ///     <para>All files in all sub-directories in the piped array of directories(C:\Videos\ and C:\Music\) will be moved to their respective parents with a confirmation prompt before moving:</para>
    ///     <code>PS C:\> "C:\Videos\","C:\Music\" | Invoke-FlattenFolder</code>
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
        /// If supplied this bypasses the confirmation prompt before both renaming and moving files.
        /// </para>
        /// </summary>
        [Parameter(Mandatory = false)]
        [Alias("F")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// <para type="description">
        /// If supplied all subdirectories will be deleted once all files have been moved.
        /// </para>
        /// </summary>
        [Parameter(Mandatory = false)]
        [Alias("DS")]
        public SwitchParameter DeleteSubDirectories { get; set; }

        #endregion

        private List<string> _dirs;
        private bool _isValid;

        /// <summary>
        /// Implements the <see cref="BeginProcessing"/> method for <see cref="InvokeFlattenFoldersCmdlet"/>.
        /// Initialise temporary containers
        /// </summary>
        protected override void BeginProcessing()
        {
            _dirs = new List<string>();
            _isValid = true;
        }

        /// <summary>
        /// Implements the <see cref="ProcessRecord"/> method for <see cref="InvokeFlattenFoldersCmdlet"/>.
        /// Validates input directory/directories exist and builds a list of directories to process in the EndProcessing method.
        /// </summary>
        protected override void ProcessRecord()
        {
            // Check if a directory list was supplied
            if ((Directories ?? new List<string>()).Any())
            {
                Directories.ForEach(d =>
                {
                    if (System.IO.Directory.Exists(d))
                        return;

                    _isValid = false;

                    ThrowTerminatingError(new ErrorRecord(new Exception($"Directory not found: {d}, terminating."), null, ErrorCategory.InvalidArgument, null));
                });

                _dirs.AddRange(Directories);

                return;
            }

            // Check if a directory was supplied
            if (!string.IsNullOrEmpty(Directory))
            {
                if (!System.IO.Directory.Exists(Directory))
                {
                    _isValid = false;

                    ThrowTerminatingError(new ErrorRecord(new Exception($"Directory not found: {Directory}, terminating."), null, ErrorCategory.InvalidArgument, null));
                }

                _dirs.Add(Directory);

                return;
            }

            _dirs.Add(SessionState.Path.CurrentFileSystemLocation.Path);
        }

        /// <summary>
        /// Implements the <see cref="EndProcessing"/> method for <see cref="InvokeFlattenFoldersCmdlet"/>.
        /// The majority of the word cloud drawing occurs here.
        /// </summary>
        protected override void EndProcessing()
        {
            if (!_isValid)
                return;

            List<(string parentDir, string file, string name)> files = new List<(string, string, string)>();

            int subDirCount = 0;
            
            _dirs.ForEach(d =>
            {
                files.AddRange(System.IO.Directory.GetFiles(d, "*", SearchOption.AllDirectories)
                    .ToList()
                    .Select(f => (d, f, Path.GetFileName(f))));

                subDirCount += System.IO.Directory.GetDirectories(d, "*", SearchOption.AllDirectories).Length;
            });

            if (!files.Any())
                ThrowTerminatingError(new ErrorRecord(new Exception("No files found, terminating."), null, ErrorCategory.InvalidArgument, null));

            // check for duplicate file names
            List<string> duplicates = files
                .GroupBy(f => f.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            // if we have naming conflicts and the Force parameter has not been passed prompt the user to confirm file renaming
            if (duplicates.Any() && !Force)
            {
                var dupCount = 0;

                duplicates.ForEach(d =>
                {
                    dupCount += files.Count(f => f.name.Equals(d, StringComparison.OrdinalIgnoreCase));
                });

                var header = $"{dupCount} files with the same name were found. These files will have a guid appended to the file name to make them unique.";
                var question = "Are you happy to continue?";

                if (!PromptYesNo(header, question))
                    return;
            }

            // if the Force parameter has not been passed calculate affected files and prompt the user to confirm the file move
            if (!Force)
            {
                // build strings required for confirmation message
                var filesText = $"{files.Count} file{(files.Count == 1 ? "" : "s")}";
                var subDirText = $"{subDirCount} sub-director{(subDirCount == 1 ? "y" : "ies")}";
                var dirText = $"{_dirs.Count} parent director{(_dirs.Count == 1 ? "y" : "ies")}{(DeleteSubDirectories ? " and delete all sub-directories" : "")}";

                var header = $"You are about to move {filesText} from {subDirText} into {dirText}";
                var question = "Are you sure you want to continue?";

                if (!PromptYesNo(header, question))
                    return;
            }

            // move all files to their associated parent directory
            foreach (var file in files)
            {
                var fileName = duplicates.Contains(file.name)
                    ? $"{file.name}_{Guid.NewGuid()}"
                    : file.name;

                File.Move(file.file, Path.Combine(file.parentDir, fileName));
            }

            // if specified, delete all sub-directories
            if (DeleteSubDirectories)
                _dirs.ForEach(d =>
                {
                    foreach (var sub in System.IO.Directory.GetDirectories(d))
                    {
                        System.IO.Directory.Delete(sub, true);
                    }
                });
        }

        /// <summary>
        /// Prompts the user with a yes/no option and returns the result as a boolean where 'yes' equals true
        /// </summary>
        /// <param name="header"></param>
        /// <param name="question"></param>
        /// <returns>
        /// Result of the user's yes/no choice, true if yes and false if no
        /// </returns>
        private bool PromptYesNo(string header, string question)
        {
            var options = new Collection<ChoiceDescription>
            {
                new ChoiceDescription("Yes"),
                new ChoiceDescription("No")
            };

            int response = Host.UI.PromptForChoice(header, question, options, 0);

            return response == 0;
        }
    }
}

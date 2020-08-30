using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PsFlattenFoldersCmdlet
{
    [Cmdlet(VerbsLifecycle.Invoke, "FlattenFolders", HelpUri = "https://github.com/trossr32/ps-flatten-folders")]
    public class InvokeFlattenFoldersCmdlet : PSCmdlet
    {
        #region Parameters

        [Parameter(Mandatory = false, Position = 0)]
        [Alias("D")]
        public string Directory { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public List<string> Directories { get; set; }

        [Parameter(Mandatory = false)]
        [Alias("F")]
        public SwitchParameter Force { get; set; }

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

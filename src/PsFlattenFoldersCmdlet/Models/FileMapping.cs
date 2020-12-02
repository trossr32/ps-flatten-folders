namespace FlattenFolders.Models
{
    internal class FileMapping
    {
        internal FileMapping(string oldFile, string newFile)
        {
            OldFile = oldFile;
            NewFile = newFile;
        }

        internal string OldFile { get; set; }
        internal string NewFile { get; set; }
    }
}

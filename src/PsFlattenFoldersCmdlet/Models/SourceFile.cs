namespace FlattenFolders.Models
{
    internal class SourceFile
    {
        internal SourceFile(string parentDir, string file, string name)
        {
            ParentDir = parentDir;
            File = file;
            Name = name;
        }

        internal string ParentDir { get; set; }
        internal string File { get; set; }
        internal string Name { get; set; }
    }
}

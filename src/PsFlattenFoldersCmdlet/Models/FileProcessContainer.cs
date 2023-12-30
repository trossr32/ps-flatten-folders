using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlattenFolders.Models;

internal class FileProcessContainer
{
    internal FileProcessContainer()
    {
        Files = [];
        SourceDirectories = [];
    }

    internal List<SourceFile> Files { get; set; }
    internal List<string> SourceDirectories { get; set; }
    internal List<FileMapping> FileMappings { get; private set; }
    internal int SubDirectoryCount { get; set; }

    internal void BuildDuplicatesAndFileMappings()
    {
        var duplicates = Files
            .GroupBy(f => f.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        FileMappings =
            (from file in Files
                let fileName = duplicates.Contains(file.Name)
                    ? $"{Path.GetFileNameWithoutExtension(file.Name)}_{Guid.NewGuid()}{Path.GetExtension(file.Name)}"
                    : file.Name
                select new FileMapping(file.File, Path.Combine(file.ParentDir, fileName)))
            .ToList();
    }
}
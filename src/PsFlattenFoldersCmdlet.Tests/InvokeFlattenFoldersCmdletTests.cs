using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FlattenFolders;
using NUnit.Framework;
using PsFlattenFoldersCmdlet.Tests.Helpers;

namespace PsFlattenFoldersCmdlet.Tests;

public partial class Tests
{
    private const string TestRoot = @"C:/Temp";
    private static readonly string[] TestRootDirs = [$"{TestRoot}/PsFlattenFoldersCmdletTest", $"{TestRoot}/PsFlattenFoldersCmdletTest_Multiple"];
    private const int DirectoryCount = 8;

    private readonly List<(string parent, List<(string sub, List<string> files)> subs)> _config =
    [
        (parent: "a", [
            (sub: "a", [
                "Duplicate.txt",
                $"{Guid.NewGuid()}.txt"
            ]),

            (sub: "b", [
                "Duplicate.txt",
                $"{Guid.NewGuid()}.txt"
            ]),

            (sub: "c", [$"{Guid.NewGuid()}.txt"])
        ]),

        (parent: "b", [
            (sub: "a", [
                "Duplicate.txt",
                $"{Guid.NewGuid()}.txt"
            ]),

            (sub: "b", [$"{Guid.NewGuid()}.txt"])
        ]),

        (parent: "c", [])
    ];

    private int _fileCount;
    private int _duplicateCount;

    [SetUp]
    public void Setup()
    {
        DeleteTestDirectories();

        foreach (var dir in TestRootDirs)
        {
            Directory.CreateDirectory(dir);

            _fileCount = 0;
            _duplicateCount = 0;

            foreach (var (parent, subs) in _config)
            {
                Directory.CreateDirectory(Path.Combine(dir, parent));

                if (subs.Count is 0)
                    continue;

                foreach (var sub in subs)
                {
                    Directory.CreateDirectory(Path.Combine(dir, parent, sub.sub));

                    if (sub.files.Count is 0)
                        continue;

                    foreach (var file in sub.files)
                    {
                        using var _ = File.Create(Path.Combine(dir, parent, sub.sub, file));

                        _fileCount++;

                        if (file.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
                            _duplicateCount++;
                    }
                }
            }
        }
    }

    [TestCase(false, DirectoryCount)]
    [TestCase(true, 0)]
    public void Directory_FlattensWithDuplicates_Test(bool deleteSubDirectories, int sourceDirectoryCount)
    {
        var results = ExecuteCmdlet(TestRootDirs[0], deleteSubDirectories);

        Assert.That(Directory.GetDirectories(TestRootDirs[0], "*", SearchOption.AllDirectories), Has.Length.EqualTo(sourceDirectoryCount));

        var files = Directory.GetFiles(TestRootDirs[0]);

        Assert.That(files, Has.Length.EqualTo(_fileCount));

        var duplicates = files.Count(f => DuplicateFileRegex().IsMatch(f));

        Assert.That(_duplicateCount, Is.EqualTo(duplicates));
    }

    [TestCase(false, DirectoryCount)]
    [TestCase(true, 0)]
    public void Directories_FlattensWithDuplicates_Test(bool deleteSubDirectories, int sourceDirectoryCount)
    {
        var results = ExecuteCmdlet(TestRootDirs, deleteSubDirectories);

        foreach (var dir in TestRootDirs)
        {
            Assert.That(Directory.GetDirectories(dir, "*", SearchOption.AllDirectories), Has.Length.EqualTo(sourceDirectoryCount));

            var files = Directory.GetFiles(dir);

            Assert.That(files, Has.Length.EqualTo(_fileCount));

            var duplicates = files.Count(f => DuplicateFileRegex().IsMatch(f));

            Assert.That(_duplicateCount, Is.EqualTo(duplicates));
        }
    }

    [Test]
    public void Directory_FlattensWithDuplicatesWhatIf_Test()
    {
        var results = ExecuteCmdlet(TestRootDirs[0], false, true);

        var actual = results as object[] ?? results.ToArray();

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Has.Length.EqualTo(1));
        Assert.That(actual[0], Has.Count.EqualTo(WhatIfOutLinesCount(1, _fileCount)));
    }

    [Test]
    public void Directories_FlattensWithDuplicatesWhatIf_Test()
    {
        var results = ExecuteCmdlet(TestRootDirs, false, true);

        var actual = results as object[] ?? results.ToArray();

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Has.Length.EqualTo(1));
        Assert.That(actual[0], Has.Count.EqualTo(WhatIfOutLinesCount(TestRootDirs.Length, _fileCount)));
    }

    [TearDown]
    public void TearDown() => DeleteTestDirectories();

    /// <summary>
    /// Executes InvokeFlattenFoldersCmdlet.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="deleteSubDirectories"></param>
    /// <param name="whatIf"></param>
    /// <returns></returns>
    private static IEnumerable<object> ExecuteCmdlet(string directory, bool deleteSubDirectories, bool whatIf = false) =>
        ExecuteCmdlet(new InvokeFlattenFoldersCmdlet
        {
            Directory = directory,
            DeleteSubDirectories = deleteSubDirectories,
            WhatIf = whatIf
        });

    /// <summary>
    /// Executes InvokeFlattenFoldersCmdlet.
    /// </summary>
    /// <param name="directories"></param>
    /// <param name="deleteSubDirectories"></param>
    /// <param name="whatIf"></param>
    /// <returns></returns>
    private static IEnumerable<object> ExecuteCmdlet(string[] directories, bool deleteSubDirectories, bool whatIf = false) =>
        ExecuteCmdlet(new InvokeFlattenFoldersCmdlet
        {
            Directories = [.. directories],
            DeleteSubDirectories = deleteSubDirectories,
            WhatIf = whatIf
        });

    /// <summary>
    /// Executes InvokeFlattenFoldersCmdlet.
    /// </summary>
    /// <param name="cmdlet"></param>
    /// <returns></returns>
    private static IEnumerable<object> ExecuteCmdlet(InvokeFlattenFoldersCmdlet cmdlet)
    {
        var psEmulator = new PowershellEmulator();

        cmdlet.CommandRuntime = psEmulator;
        cmdlet.ProcessInternal();

        return psEmulator.OutputObjects;
    }

    /// <summary>
    /// Deletes test directories.
    /// </summary>
    private static void DeleteTestDirectories() =>
        Array.ForEach(TestRootDirs, d =>
        {
            if (Directory.Exists(d))
                Directory.Delete(d, true);
        });

    /// <summary>
    /// Calculates expected output lines for WhatIf command.
    /// </summary>
    /// <param name="directoryCount"></param>
    /// <param name="fileCount"></param>
    /// <returns></returns>
    private static int WhatIfOutLinesCount(int directoryCount, int fileCount) =>
        directoryCount + 11 + (fileCount * directoryCount);

    [GeneratedRegex(@"Duplicate_[0-9A-Fa-f\-]{36}\.txt")]
    private static partial Regex DuplicateFileRegex();
}
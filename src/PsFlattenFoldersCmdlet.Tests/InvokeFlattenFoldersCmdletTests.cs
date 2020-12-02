using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using FlattenFolders;
using NUnit.Framework;

namespace PsFlattenFoldersCmdlet.Tests
{
    public class Tests
    {
        private const string TestRootDir = @"C:/temp/PsFlattenFoldersCmdletTest";

        private readonly List<(string parent, List<(string sub, List<string> files)> subs)> _config = new List<(string parent, List<(string sub, List<string> files)> subs)>
        {
            (parent: "a", new List<(string sub, List<string> files)>
            {
                (sub: "a", new List<string>
                {
                    "Duplicate.txt",
                    $"{Guid.NewGuid()}.txt"
                }),
                (sub: "b", new List<string>
                {
                    "Duplicate.txt",
                    $"{Guid.NewGuid()}.txt"
                }),
                (sub: "c", new List<string>
                {
                    $"{Guid.NewGuid()}.txt"
                })
            }),
            (parent: "b", new List<(string sub, List<string> files)>
            {
                (sub: "a", new List<string>
                {
                    "Duplicate.txt",
                    $"{Guid.NewGuid()}.txt"
                }),
                (sub: "b", new List<string>
                {
                    $"{Guid.NewGuid()}.txt"
                })
            }),
            (parent: "c", new List<(string sub, List<string> files)>())
        };

        private int _fileCount;
        private int _duplicateCount;

        [OneTimeSetUp]
        public void Setup()
        {
            if (Directory.Exists(TestRootDir))
                Directory.Delete(TestRootDir, true);

            Directory.CreateDirectory(TestRootDir);

            _fileCount = 0;
            _duplicateCount = 0;

            foreach (var config in _config)
            {
                Directory.CreateDirectory(Path.Combine(TestRootDir, config.parent));

                if (!config.subs.Any())
                    continue;

                foreach (var sub in config.subs)
                {
                    Directory.CreateDirectory(Path.Combine(TestRootDir, config.parent, sub.sub));

                    if (!sub.files.Any())
                        continue;

                    foreach (var file in sub.files)
                    {
                        File.Create(Path.Combine(TestRootDir, config.parent, sub.sub, file));

                        _fileCount++;

                        if (file.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
                            _duplicateCount++;
                    }
                }
            }
        }

        [Test]
        public void Test1()
        {
            var cmdlet = new InvokeFlattenFoldersCmdlet
            {
                Directory = TestRootDir
            };

            // TODO - This only works when the cmdlet inherits from Cmdlet (not PSCmdlet). However the cmdlet needs to inherit
            // TODO   from PSCmdlet in order to access the host and prompt the user. Resolution required.
            var results = cmdlet.Invoke().OfType<object>().ToList();

            Assert.False(Directory.GetDirectories(TestRootDir).Any());

            string[] files = Directory.GetFiles(TestRootDir);

            Assert.AreEqual(files.Length, _fileCount);

            int duplicates = files.Count(f => Regex.IsMatch(f, @"Duplicate_[0-9A-Fa-f\-]{36}\.txt"));

            Assert.AreEqual(_duplicateCount, duplicates);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (Directory.Exists(TestRootDir))
                Directory.Delete(TestRootDir, true);
        }
    }
}
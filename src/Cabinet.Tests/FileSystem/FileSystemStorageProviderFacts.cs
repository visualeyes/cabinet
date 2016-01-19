using Cabinet.Core.Providers;
using Cabinet.FileSystem;
using Cabinet.Tests.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.FileSystem {
    public class FileSystemStorageProviderFacts {
        private readonly MockFileSystem mockFileSystem;

        public FileSystemStorageProviderFacts() {
            this.mockFileSystem = new MockFileSystem();
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Exists_Missing_File(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            Assert.False(this.mockFileSystem.FileExists(expectedFilePath));

            var exists = await provider.ExistsAsync(key, config);

            Assert.False(exists);
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Get_File_Existing_File(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            this.mockFileSystem.AddFile(expectedFilePath, new MockFileData("test"));

            Assert.True(this.mockFileSystem.FileExists(expectedFilePath));

            var fileInfo = await provider.GetFileAsync(key, config);

            Assert.Equal(FileSystemStorageProvider.ProviderType, fileInfo.ProviderType);
            Assert.Equal(expectedFileKey, fileInfo.Key);
            Assert.True(fileInfo.Exists);

            using (var stream = fileInfo.GetFileReadStream()) {
                Assert.NotNull(stream);
            }
        }

        [Theory]
        [MemberData("GetListTestPaths")]
        public async Task List_Keys(string basePath, IDictionary<string, string> files, string keyPrefix, bool recursive, IDictionary<string, string> expectedFiles) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            foreach (var file in files) {
                var filePath = Path.Combine(basePath, file.Key);
                this.mockFileSystem.AddFile(filePath, file.Value);
            }

            var keys = await provider.ListKeysAsync(config, keyPrefix: keyPrefix, recursive: recursive);

            Assert.Equal(expectedFiles.Count, keys.Count());
            Assert.Equal(expectedFiles.Keys, keys);
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Get_File_Missing_File(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            Assert.False(this.mockFileSystem.FileExists(expectedFilePath));

            var fileInfo = await provider.GetFileAsync(key, config);

            Assert.Equal(FileSystemStorageProvider.ProviderType, fileInfo.ProviderType);
            Assert.Equal(expectedFileKey, fileInfo.Key);
            Assert.False(fileInfo.Exists);
            Assert.Throws<InvalidOperationException>(() => fileInfo.GetFileReadStream());
        }

        [Theory]
        [MemberData("GetListTestPaths")]
        public async Task Get_Files(string basePath, IDictionary<string, string> files, string keyPrefix, bool recursive, IDictionary<string, string> expectedFiles) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            foreach (var file in files) {
                var filePath = Path.Combine(basePath, file.Key);
                this.mockFileSystem.AddFile(filePath, file.Value);
            }

            var actualFiles = await provider.GetFilesAsync(config, keyPrefix: keyPrefix, recursive: recursive);

            Assert.Equal(expectedFiles.Count, actualFiles.Count());
            Assert.Equal(expectedFiles.Keys, actualFiles.Select(a => a.Key));
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Save_File(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            Assert.False(this.mockFileSystem.FileExists(expectedFilePath));
            string content = "test";

            using (var contentStream = GetStream(content)) {
                var result = await provider.SaveFileAsync(key, contentStream, HandleExistingMethod.Throw, config);

                Assert.Null(result.Exception);
                Assert.True(result.Success);
                Assert.True(this.mockFileSystem.FileExists(expectedFilePath));

                var mockFile = this.mockFileSystem.GetFile(expectedFilePath);
                Assert.Equal(content, mockFile.TextContents);
            }
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Save_File_Overwrites_Existing(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);
            string content = "test";

            this.mockFileSystem.AddFile(expectedFilePath, new MockFileData(content + "-old"));

            Assert.True(this.mockFileSystem.FileExists(expectedFilePath));

            using (var contentStream = GetStream(content)) {
                var result = await provider.SaveFileAsync(key, contentStream, HandleExistingMethod.Overwrite, config);

                Assert.Null(result.Exception);
                Assert.True(result.Success);
                Assert.True(this.mockFileSystem.FileExists(expectedFilePath));
                
                var mockFile = this.mockFileSystem.GetFile(expectedFilePath);
                Assert.Equal(content, mockFile.TextContents);
            }
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Save_File_No_Overwrites_Throws(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);
            string content = "test";

            this.mockFileSystem.AddFile(expectedFilePath, new MockFileData(content + "-old"));

            Assert.True(this.mockFileSystem.FileExists(expectedFilePath));

            using (var contentStream = GetStream(content)) {

                await Assert.ThrowsAsync<ApplicationException>(async () => {
                    await provider.SaveFileAsync(key, contentStream, HandleExistingMethod.Throw, config);
                });
            }
        }

        [Theory]
        [MemberData("GetMoveTestPaths")]
        public async Task Move_FileSystem_File_To_Missing(string basePath, string fromKey, string fromPath, string toKey, string toPath) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);
            string content = "test";

            this.mockFileSystem.AddFile(fromPath, new MockFileData(content));

            Assert.True(this.mockFileSystem.FileExists(fromPath));
            Assert.False(this.mockFileSystem.FileExists(toPath));

            var fromFile = await provider.GetFileAsync(fromKey, config);

            var result = await provider.MoveFileAsync(fromFile, toKey, HandleExistingMethod.Throw, config);

            Assert.Null(result.Exception);
            Assert.True(result.Success);

            Assert.False(this.mockFileSystem.FileExists(fromPath));
            Assert.True(this.mockFileSystem.FileExists(toPath));
        }

        [Theory]
        [MemberData("GetMoveTestPaths")]
        public async Task Move_FileSystem_File_To_Existing(string basePath, string fromKey, string fromPath, string toKey, string toPath) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);
            string content = "test";

            this.mockFileSystem.AddFile(fromPath, new MockFileData(content));
            this.mockFileSystem.AddFile(toPath, new MockFileData("random-blah"));

            Assert.True(this.mockFileSystem.FileExists(fromPath));
            Assert.True(this.mockFileSystem.FileExists(toPath));

            var fromFile = await provider.GetFileAsync(fromKey, config);

            var result = await provider.MoveFileAsync(fromFile, toKey, HandleExistingMethod.Overwrite, config);

            Assert.Null(result.Exception);
            Assert.True(result.Success);

            Assert.False(this.mockFileSystem.FileExists(fromPath));
            Assert.True(this.mockFileSystem.FileExists(toPath));
        }

        [Theory]
        [MemberData("GetMoveTestPaths")]
        public async Task Move_FileSystem_File_To_Existing_Throws(string basePath, string fromKey, string fromPath, string toKey, string toPath) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);
            string content = "test";

            this.mockFileSystem.AddFile(fromPath, new MockFileData(content));
            this.mockFileSystem.AddFile(toPath, new MockFileData("random-blah"));

            Assert.True(this.mockFileSystem.FileExists(fromPath));
            Assert.True(this.mockFileSystem.FileExists(toPath));

            var fromFile = await provider.GetFileAsync(fromKey, config);
            
            await Assert.ThrowsAsync<ApplicationException>(async () => {
                await provider.MoveFileAsync(fromFile, toKey, HandleExistingMethod.Throw, config);
            });
        }

        [Theory]
        [InlineData(@"c:\foo", @"to.txt", @"c:\foo\to.txt")]
        public async Task Move_Other_File_To_Missing(string basePath, string toKey, string toPath) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);
            string content = "test";
            
            Assert.False(this.mockFileSystem.FileExists(toPath));

            var fromFile = new TestCabinetFileInfo("from.txt", true, GetStream(content));

            var result = await provider.MoveFileAsync(fromFile, toKey, HandleExistingMethod.Throw, config);

            Assert.Null(result.Exception);
            Assert.True(result.Success);
            
            Assert.True(this.mockFileSystem.FileExists(toPath));
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Delete_Existing_File(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            this.mockFileSystem.AddFile(expectedFilePath, new MockFileData("test"));

            Assert.True(this.mockFileSystem.FileExists(expectedFilePath));

            var result = await provider.DeleteFileAsync(key, config);

            Assert.Null(result.Exception);
            Assert.True(result.Success);
            Assert.False(result.AlreadyDeleted);
            Assert.False(this.mockFileSystem.FileExists(expectedFilePath));
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Delete_Missing_File(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            Assert.False(this.mockFileSystem.FileExists(expectedFilePath));

            var result = await provider.DeleteFileAsync(key, config);

            Assert.Null(result.Exception);
            Assert.True(result.Success);
            Assert.True(result.AlreadyDeleted);
            Assert.False(this.mockFileSystem.FileExists(expectedFilePath));
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Exists_Existing_File(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            this.mockFileSystem.AddFile(expectedFilePath, new MockFileData("test"));

            Assert.True(this.mockFileSystem.FileExists(expectedFilePath));

            var exists = await provider.ExistsAsync(key, config);

            Assert.True(exists);
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public void Get_File_Path(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            var fileInfo = provider.GetFileInfo(key, config);

            Assert.Equal(expectedFilePath, fileInfo.FullName);
        }

        [Theory]
        [InlineData(@"c:\foo", @"..\blah\bar\baz", @"c:\foo\bar\baz")]
        [InlineData(@"c:\foo", @".\..\..\blah\bar\baz", @"c:\foo\bar\baz")]
        [InlineData(@"c:\foo", @"../blah/bar/baz", @"c:\foo\bar\baz")]
        [InlineData(@"c:\foo", @"C:/blah/bar/baz", @"c:\foo\bar\baz")]
        public void Get_Dangerous_File_Path_Throws(string basePath, string key, string expectedFilePath) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            Assert.Throws<ArgumentException>(() => {
                provider.GetFileInfo(key, config);
            });
        }

        private FileSystemStorageProvider GetProvider(string basePath) {            
            var provider = new FileSystemStorageProvider(() => this.mockFileSystem);
            
            return provider;
        }

        private FileSystemCabinetConfig GetConfig(string basePath) {
            var config = new FileSystemCabinetConfig() {
                Directory = basePath
            };

            return config;
        }

        public static object[] GetSafeTestPaths() {
            return new object[] {
                new object[] { @"c:\foo", @"file.txt", @"c:\foo\file.txt", @"file.txt" },
                new object[] { @"c:\foo", @"bar\file.txt", @"c:\foo\bar\file.txt", @"bar/file.txt" },
                new object[] { @"c:\foo", @"bar\baz", @"c:\foo\bar\baz", @"bar/baz" },
                new object[] { @"c:\foo", @"./bar/baz", @"c:\foo\bar\baz", @"bar/baz" },
                new object[] { @"c:\foo", @"../foo/bar/baz", @"c:\foo\bar\baz", @"bar/baz" },
            };
        }

        public static object[] GetMoveTestPaths() {
            return new object[] {
                new object[] { @"c:\foo", @"from.txt", @"c:\foo\from.txt", @"to.txt", @"c:\foo\to.txt" },
            };
        }

        public static object[] GetListTestPaths() {
            string baseDir = @"C:\data";

            var files = new Dictionary<string, string> {
                { "file.txt", "test-file" },
                { @"bar\one.txt", "one" },
                { @"bar\two.txt", "two" },
                { @"bar\baz\three", "three" },
                { @"foo\one.txt", "one" },
            };


            return new object[] {
                new object[] { baseDir, files, "", true, new Dictionary<string, string> {
                        { "file.txt", "test-file" },
                        { @"bar/one.txt", "one" },
                        { @"bar/two.txt", "two" },
                        { @"bar/baz/three", "three" },
                        { @"foo/one.txt", "one" },
                    }
                },
                new object[] { baseDir, files, "", false, new Dictionary<string, string> {
                        { "file.txt", "test-file" },
                    }
                },
                new object[] { baseDir, files, "bar", true, new Dictionary<string, string> {
                        { @"bar/one.txt", "one" },
                        { @"bar/two.txt", "two" },
                        { @"bar/baz/three", "three" },
                    }
                },
                new object[] { baseDir, files, "bar", false, new Dictionary<string, string> {
                        { @"bar/one.txt", "one" },
                        { @"bar/two.txt", "two" },
                    }
                },
                new object[] { baseDir, files, @"bar\baz", false, new Dictionary<string, string> {
                        { @"bar/baz/three", "three" },
                    }
                },
            };
        }

        private static Stream GetStream(string str) {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}

using Cabinet.Core;
using Cabinet.Core.Providers;
using Cabinet.FileSystem;
using Cabinet.Tests.Core;
using Moq;
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
        private const string ValidBasePath = @"C:\tests";

        private readonly MockFileSystem mockFileSystem;

        public FileSystemStorageProviderFacts() {
            this.mockFileSystem = new MockFileSystem();
        }

        [Fact]
        public void Provider_Type() {
            IStorageProvider<FileSystemCabinetConfig> provider = GetProvider(ValidBasePath);
            Assert.Equal(FileSystemStorageProvider.ProviderType, provider.ProviderType);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Exists_Empty_Key_Throws(string key) {
            var provider = GetProvider(ValidBasePath);
            var config = GetConfig(ValidBasePath);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.ExistsAsync(key, config));
        }

        [Fact]
        public async Task Exists_Null_Config_Throws() {
            var provider = GetProvider(ValidBasePath);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.ExistsAsync("key", null));
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
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task GetFile_Empty_Key_Throws(string key) {
            var provider = GetProvider(ValidBasePath);
            var config = GetConfig(ValidBasePath);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.GetFileAsync(key, config));
        }

        [Fact]
        public async Task GetFile_Null_Config_Throws() {
            var provider = GetProvider(ValidBasePath);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.GetFileAsync("key", null));
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
        }

        [Fact]
        public async Task List_Keys_Null_Config_Throws() {
            var provider = GetProvider(ValidBasePath);
            FileSystemCabinetConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.ListKeysAsync(config));
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

        [Fact]
        public async Task Get_Files_Null_Config_Throws() {
            var provider = GetProvider(ValidBasePath);
            FileSystemCabinetConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.GetFilesAsync(config));
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

        [Fact]
        public async Task Get_Files_Missing_Directory() {
            string basePath = @"C:\test";

            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);

            var actualFiles = await provider.GetFilesAsync(config, keyPrefix: "missingDir", recursive: true);

            Assert.Empty(actualFiles);
        }


        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Open_Read_Stream_Empty_Key_Throws(string key) {
            var provider = GetProvider(ValidBasePath);
            var config = GetConfig(ValidBasePath);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.OpenReadStreamAsync(key, config));
        }

        [Fact]
        public async Task Open_Read_Stream_Null_Config_Throws() {
            var provider = GetProvider(ValidBasePath);
            FileSystemCabinetConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.OpenReadStreamAsync("key", config));
        }

        [Theory]
        [InlineData(@"C:\tests\test.txt", "test")]
        public async Task Open_Read_Stream(string key, string content) {
            var provider = GetProvider(ValidBasePath);
            var config = GetConfig(ValidBasePath);

            this.mockFileSystem.AddFile(key, new MockFileData(content));

            using (var stream = await provider.OpenReadStreamAsync(key, config)) {
                using (var reader = new StreamReader(stream)) {
                    string actualContents = reader.ReadToEnd();

                    Assert.Equal(content, actualContents);
                }
            }
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Save_File_Stream_Empty_Key_Throws(string key) {
            var provider = GetProvider(ValidBasePath);
            var config = GetConfig(ValidBasePath);
            var mockProgress = new Mock<IProgress<WriteProgress>>();
            using (var stream = new MemoryStream()) {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.SaveFileAsync(key, stream, HandleExistingMethod.Overwrite, mockProgress.Object, config));
            }
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Save_File_FilePath_Empty_Key_Throws(string key) {
            var provider = GetProvider(ValidBasePath);
            var config = GetConfig(ValidBasePath);
            var mockProgress = new Mock<IProgress<WriteProgress>>();
            using (var stream = new MemoryStream()) {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.SaveFileAsync(key, @"C:\tests\blah.txt", HandleExistingMethod.Overwrite, mockProgress.Object, config));
            }
        }

        [Fact]
        public async Task Save_File_Stream_Null_Stream_Throws() {
            var provider = GetProvider(ValidBasePath);
            var config = GetConfig(ValidBasePath);
            var mockProgress = new Mock<IProgress<WriteProgress>>();
            Stream stream = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.SaveFileAsync("key", stream, HandleExistingMethod.Overwrite, mockProgress.Object, config));
        }

        [Fact]
        public async Task Save_File_Stream_Null_Config_Throws() {
            var provider = GetProvider(ValidBasePath);
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            using (var stream = new MemoryStream()) {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.SaveFileAsync("key", stream, HandleExistingMethod.Overwrite, mockProgress.Object, null));
            }
        }

        [Fact]
        public async Task Save_File_FilePath_Null_Config_Throws() {
            var provider = GetProvider(ValidBasePath);
            var mockProgress = new Mock<IProgress<WriteProgress>>();
            
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.SaveFileAsync("key", @"C:\foo", HandleExistingMethod.Overwrite, mockProgress.Object, null));
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Save_File_Stream(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            Assert.False(this.mockFileSystem.FileExists(expectedFilePath));
            string content = "test";

            using (var contentStream = GetStream(content)) {
                var result = await provider.SaveFileAsync(key, contentStream, HandleExistingMethod.Throw, mockProgress.Object, config);

                Assert.Null(result.Exception);
                Assert.True(result.Success);
                Assert.True(this.mockFileSystem.FileExists(expectedFilePath));

                var mockFile = this.mockFileSystem.GetFile(expectedFilePath);
                Assert.Equal(content, mockFile.TextContents);
            }
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Save_File_Stream_Overwrites_Existing(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            string content = "test";

            this.mockFileSystem.AddFile(expectedFilePath, new MockFileData(content + "-old"));

            Assert.True(this.mockFileSystem.FileExists(expectedFilePath));

            using (var contentStream = GetStream(content)) {
                var result = await provider.SaveFileAsync(key, contentStream, HandleExistingMethod.Overwrite, mockProgress.Object, config);

                Assert.Null(result.Exception);
                Assert.True(result.Success);
                Assert.True(this.mockFileSystem.FileExists(expectedFilePath));
                
                var mockFile = this.mockFileSystem.GetFile(expectedFilePath);
                Assert.Equal(content, mockFile.TextContents);
            }
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Save_File_Stream_No_Overwrites_Throws(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            string content = "test";

            this.mockFileSystem.AddFile(expectedFilePath, new MockFileData(content + "-old"));

            Assert.True(this.mockFileSystem.FileExists(expectedFilePath));

            using (var contentStream = GetStream(content)) {

                await Assert.ThrowsAsync<ApplicationException>(async () => {
                    await provider.SaveFileAsync(key, contentStream, HandleExistingMethod.Throw, mockProgress.Object, config);
                });
            }
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Save_File_File_Path(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            Assert.False(this.mockFileSystem.FileExists(expectedFilePath));

            string filePath = @"C:\foo\blah.txt";
            string content = "test";

            this.mockFileSystem.AddFile(filePath, new MockFileData(content));

            var result = await provider.SaveFileAsync(key, filePath, HandleExistingMethod.Throw, mockProgress.Object, config);

            Assert.Null(result.Exception);
            Assert.True(result.Success);
            Assert.True(this.mockFileSystem.FileExists(expectedFilePath));

            var mockFile = this.mockFileSystem.GetFile(expectedFilePath);
            Assert.Equal(content, mockFile.TextContents);
        }

        [Theory]
        [MemberData("GetSafeTestPaths")]
        public async Task Save_File_File_Path_Missing_File(string basePath, string key, string expectedFilePath, string expectedFileKey) {
            var provider = GetProvider(basePath);
            var config = GetConfig(basePath);
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            Assert.False(this.mockFileSystem.FileExists(expectedFilePath));

            string filePath = @"C:\foo\blah.txt";

            var result = await provider.SaveFileAsync(key, filePath, HandleExistingMethod.Throw, mockProgress.Object, config);

            Assert.Null(result.Exception);
            Assert.False(result.Success);
            Assert.Equal("File does not exist at path " + filePath, result.GetErrorMessage());
            Assert.False(this.mockFileSystem.FileExists(expectedFilePath));
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task MoveFile_Empty_SourceKey_Throws(string sourcekey) {
            var provider = GetProvider(ValidBasePath);
            var config = GetConfig(ValidBasePath);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.MoveFileAsync(sourcekey, "destkey", HandleExistingMethod.Overwrite, config));
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task MoveFile_Empty_DestKey_Throws(string destkey) {
            var provider = GetProvider(ValidBasePath);
            var config = GetConfig(ValidBasePath);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.MoveFileAsync("sourcekey", destkey, HandleExistingMethod.Overwrite, config));
        }

        [Fact]
        public async Task MoveFile_Null_Config_Throws() {
            var provider = GetProvider(ValidBasePath);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.MoveFileAsync("sourcekey", "destkey", HandleExistingMethod.Overwrite, null));
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
            
            var result = await provider.MoveFileAsync(fromKey, toKey, HandleExistingMethod.Throw, config);

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
            
            var result = await provider.MoveFileAsync(fromKey, toKey, HandleExistingMethod.Overwrite, config);

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
                        
            await Assert.ThrowsAsync<ApplicationException>(async () => {
                await provider.MoveFileAsync(fromKey, toKey, HandleExistingMethod.Throw, config);
            });
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task DeleteFile_Empty_Key_Throws(string key) {
            var provider = GetProvider(ValidBasePath);
            var config = GetConfig(ValidBasePath);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.DeleteFileAsync(key, config));
        }

        [Fact]
        public async Task Delete_Null_Config_Throws() {
            var provider = GetProvider(ValidBasePath);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.DeleteFileAsync("key", null));
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
        [InlineData(null), InlineData(""), InlineData(" ")]
        public void GetFileInfo_Empty_Key_Throws(string key) {
            var provider = GetProvider(ValidBasePath);
            var config = GetConfig(ValidBasePath);
            Assert.Throws<ArgumentNullException>(() => provider.GetFileInfo(key, config));
        }

        [Fact]
        public void GetFileInfo_Null_Config_Throws() {
            var provider = GetProvider(ValidBasePath);
            Assert.Throws<ArgumentNullException>(() => provider.GetFileInfo("key", null));
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
            var config = new FileSystemCabinetConfig(basePath, true);

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

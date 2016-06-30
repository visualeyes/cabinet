using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Cabinet.Core.Providers;
using Cabinet.Core;
using Cabinet.Core.Results;
using System.IO;

namespace Cabinet.Tests.Core {
    public class FileCabinetFacts {
        private const string TestProviderType = "Test";

        private readonly TestProviderConfiguration config;
        private readonly Mock<IStorageProvider<TestProviderConfiguration>> mockStorageProvider;

        private readonly FileCabinet<TestProviderConfiguration> fileCabinet;

        public FileCabinetFacts() {
            this.config = new TestProviderConfiguration();
            this.mockStorageProvider = new Mock<IStorageProvider<TestProviderConfiguration>>();
            this.mockStorageProvider.SetupGet(p => p.ProviderType).Returns(TestProviderType);

            this.fileCabinet = new FileCabinet<TestProviderConfiguration>(mockStorageProvider.Object, config);
        }

        [Fact]
        public void Get_Delimiter() {
            string expectedDelimiter = "/";
            this.config.Delimiter = expectedDelimiter;

            string delimiter = this.fileCabinet.GetKeyDelimiter();
            Assert.Equal(expectedDelimiter, delimiter);
        }
        
        [Fact]
        public void Null_Provider_Throws() {
            Assert.Throws<ArgumentNullException>(() => new FileCabinet<TestProviderConfiguration>(null, this.config));
        }

        [Fact]
        public void Null_Config_Throws() {
            Assert.Throws<ArgumentNullException>(() => new FileCabinet<TestProviderConfiguration>(mockStorageProvider.Object, null));
        }

        [Theory]
        [InlineData("key", true)]
        [InlineData("key", false)]
        public async Task Exists(string key, bool expectedExists) {
            this.mockStorageProvider.Setup(s => s.ExistsAsync(key, config)).ReturnsAsync(expectedExists);

            var actualExists = await this.fileCabinet.ExistsAsync(key);

            this.mockStorageProvider.Verify(s => s.ExistsAsync(key, config), Times.Once);

            Assert.Equal(expectedExists, actualExists);
        }

        [Theory]
        [InlineData("key")]
        public async Task Get_File(string key) {
            var mockFile = new Mock<ICabinetItemInfo>();
            this.mockStorageProvider.Setup(s => s.GetItemAsync(key, config)).ReturnsAsync(mockFile.Object);

            var actualFile = await this.fileCabinet.GetItemAsync(key);

            this.mockStorageProvider.Verify(s => s.GetItemAsync(key, config), Times.Once);

            Assert.Equal(mockFile.Object, actualFile);
        }
        
        [Theory]
        [InlineData("keyPrefix", true)]
        [InlineData("", false)]
        public async Task Get_Files(string keyPrefix, bool recursive) {
            var mockFile = new Mock<ICabinetItemInfo>();
            var expectedFiles = new List<ICabinetItemInfo>() { mockFile.Object };
            this.mockStorageProvider.Setup(s => s.GetItemsAsync(config, keyPrefix, recursive)).ReturnsAsync(expectedFiles);

            var actualFiles = await this.fileCabinet.GetItemsAsync(keyPrefix: keyPrefix, recursive: recursive);

            this.mockStorageProvider.Verify(s => s.GetItemsAsync(config, keyPrefix, recursive), Times.Once);

            Assert.Equal(expectedFiles, actualFiles);
        }
        
        [Fact]
        public async Task OpenRead() {
            string key = "key";
            var mockStream = new Mock<Stream>();

            this.mockStorageProvider.Setup(p => p.OpenReadStreamAsync(key, this.config)).ReturnsAsync(mockStream.Object);

            var file = new TestCabinetFileInfo(key, true, ItemType.File) {
                ProviderType = TestProviderType
            };

            var stream = await this.fileCabinet.OpenReadStreamAsync(file.Key);

            this.mockStorageProvider.Verify(p => p.OpenReadStreamAsync(key, this.config), Times.Once);

            Assert.Equal(mockStream.Object, stream);
        }

        [Theory]
        [InlineData("keyPrefix", true)]
        [InlineData("", false)]
        public async Task List_Keys(string keyPrefix, bool recursive) {
            var expectedKeys = new List<string>() { "test" };
            this.mockStorageProvider.Setup(s => s.ListKeysAsync(config, keyPrefix, recursive)).ReturnsAsync(expectedKeys);

            var actualKeys = await this.fileCabinet.ListKeysAsync(keyPrefix: keyPrefix, recursive: recursive);

            this.mockStorageProvider.Verify(s => s.ListKeysAsync(config, keyPrefix, recursive), Times.Once);

            Assert.Equal(expectedKeys, actualKeys);
        }

        [Theory]
        [InlineData("sourceKey", "destKey", HandleExistingMethod.Overwrite)]
        [InlineData("sourceKey", "destKey", HandleExistingMethod.Skip)]
        [InlineData("sourceKey", "destKey", HandleExistingMethod.Throw)]
        public async Task Move_File(string sourceKey, string destKey, HandleExistingMethod handleExisting) {
            var mockResult = new Mock<IMoveResult>();

            this.mockStorageProvider.Setup(s => s.MoveFileAsync(sourceKey, destKey, handleExisting, config)).ReturnsAsync(mockResult.Object);

            var actualResult = await this.fileCabinet.MoveFileAsync(sourceKey, destKey, handleExisting);

            this.mockStorageProvider.Verify(s => s.MoveFileAsync(sourceKey, destKey, handleExisting, config), Times.Once);

            Assert.Equal(mockResult.Object, actualResult);
        }

        [Theory]
        [InlineData("key", HandleExistingMethod.Overwrite)]
        [InlineData("key", HandleExistingMethod.Skip)]
        [InlineData("key", HandleExistingMethod.Throw)]
        public async Task Save_File_Stream(string key, HandleExistingMethod handleExisting) {
            var mockStream = new Mock<Stream>();
            var mockResult = new Mock<ISaveResult>();
            var mockProgress = new Mock<IProgress<IWriteProgress>>();

            this.mockStorageProvider.Setup(s => s.SaveFileAsync(key, mockStream.Object, handleExisting, mockProgress.Object, config)).ReturnsAsync(mockResult.Object);

            var actualResult = await this.fileCabinet.SaveFileAsync(key, mockStream.Object, handleExisting, mockProgress.Object);

            this.mockStorageProvider.Verify(s => s.SaveFileAsync(key, mockStream.Object, handleExisting, mockProgress.Object, config), Times.Once);

            Assert.Equal(mockResult.Object, actualResult);
        }

        [Theory]
        [InlineData("key", HandleExistingMethod.Overwrite)]
        [InlineData("key", HandleExistingMethod.Skip)]
        [InlineData("key", HandleExistingMethod.Throw)]
        public async Task Save_File_Path(string key, HandleExistingMethod handleExisting) {
            string filePath = "C:\test";
            var mockResult = new Mock<ISaveResult>();
            var mockProgress = new Mock<IProgress<IWriteProgress>>();

            this.mockStorageProvider.Setup(s => s.SaveFileAsync(key, filePath, handleExisting, mockProgress.Object, config)).ReturnsAsync(mockResult.Object);

            var actualResult = await this.fileCabinet.SaveFileAsync(key, filePath, handleExisting, mockProgress.Object);

            this.mockStorageProvider.Verify(s => s.SaveFileAsync(key, filePath, handleExisting, mockProgress.Object, config), Times.Once);

            Assert.Equal(mockResult.Object, actualResult);
        }

        [Theory]
        [InlineData("key")]
        public async Task Delete_File(string key) {
            var mockResult = new Mock<IDeleteResult>();

            this.mockStorageProvider.Setup(s => s.DeleteFileAsync(key, config)).ReturnsAsync(mockResult.Object);

            var actualResult = await this.fileCabinet.DeleteFileAsync(key);

            this.mockStorageProvider.Verify(s => s.DeleteFileAsync(key, config), Times.Once);

            Assert.Equal(mockResult.Object, actualResult);
        }
    }
}

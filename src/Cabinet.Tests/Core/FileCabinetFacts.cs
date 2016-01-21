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
        private readonly Mock<ITestProviderConfiguration> mockConfig;
        private readonly Mock<IStorageProvider<ITestProviderConfiguration>> mockStorageProvider;

        private readonly FileCabinet<ITestProviderConfiguration> fileCabinet;

        public FileCabinetFacts() {
            this.mockConfig = new Mock<ITestProviderConfiguration>();
            this.mockStorageProvider = new Mock<IStorageProvider<ITestProviderConfiguration>>();

            this.fileCabinet = new FileCabinet<ITestProviderConfiguration>(mockStorageProvider.Object, mockConfig.Object);
        }
        
        [Fact]
        public void Null_Provider_Throws() {
            Assert.Throws<ArgumentNullException>(() => new FileCabinet<ITestProviderConfiguration>(null, this.mockConfig.Object));
        }

        [Fact]
        public void Null_Config_Throws() {
            Assert.Throws<ArgumentNullException>(() => new FileCabinet<ITestProviderConfiguration>(mockStorageProvider.Object, null));
        }

        [Theory]
        [InlineData("key", true)]
        [InlineData("key", false)]
        public async Task Exists(string key, bool expectedExists) {
            this.mockStorageProvider.Setup(s => s.ExistsAsync(key, mockConfig.Object)).ReturnsAsync(expectedExists);

            var actualExists = await this.fileCabinet.ExistsAsync(key);

            this.mockStorageProvider.Verify(s => s.ExistsAsync(key, mockConfig.Object), Times.Once);

            Assert.Equal(expectedExists, actualExists);
        }

        [Theory]
        [InlineData("key")]
        public async Task Get_File(string key) {
            var mockFile = new Mock<ICabinetFileInfo>();
            this.mockStorageProvider.Setup(s => s.GetFileAsync(key, mockConfig.Object)).ReturnsAsync(mockFile.Object);

            var actualFile = await this.fileCabinet.GetFileAsync(key);

            this.mockStorageProvider.Verify(s => s.GetFileAsync(key, mockConfig.Object), Times.Once);

            Assert.Equal(mockFile.Object, actualFile);
        }
        
        [Theory]
        [InlineData("keyPrefix", true)]
        [InlineData("", false)]
        public async Task Get_Files(string keyPrefix, bool recursive) {
            var mockFile = new Mock<ICabinetFileInfo>();
            var expectedFiles = new List<ICabinetFileInfo>() { mockFile.Object };
            this.mockStorageProvider.Setup(s => s.GetFilesAsync(mockConfig.Object, keyPrefix, recursive)).ReturnsAsync(expectedFiles);

            var actualFiles = await this.fileCabinet.GetFilesAsync(keyPrefix: keyPrefix, recursive: recursive);

            this.mockStorageProvider.Verify(s => s.GetFilesAsync(mockConfig.Object, keyPrefix, recursive), Times.Once);

            Assert.Equal(expectedFiles, actualFiles);
        }

        [Theory]
        [InlineData("keyPrefix", true)]
        [InlineData("", false)]
        public async Task List_Keys(string keyPrefix, bool recursive) {
            var expectedKeys = new List<string>() { "test" };
            this.mockStorageProvider.Setup(s => s.ListKeysAsync(mockConfig.Object, keyPrefix, recursive)).ReturnsAsync(expectedKeys);

            var actualKeys = await this.fileCabinet.ListKeysAsync(keyPrefix: keyPrefix, recursive: recursive);

            this.mockStorageProvider.Verify(s => s.ListKeysAsync(mockConfig.Object, keyPrefix, recursive), Times.Once);

            Assert.Equal(expectedKeys, actualKeys);
        }

        [Theory]
        [InlineData("sourceKey", "destKey", HandleExistingMethod.Overwrite)]
        [InlineData("sourceKey", "destKey", HandleExistingMethod.Skip)]
        [InlineData("sourceKey", "destKey", HandleExistingMethod.Throw)]
        public async Task Move_File(string sourceKey, string destKey, HandleExistingMethod handleExisting) {
            var mockResult = new Mock<IMoveResult>();

            this.mockStorageProvider.Setup(s => s.MoveFileAsync(sourceKey, destKey, handleExisting, mockConfig.Object)).ReturnsAsync(mockResult.Object);

            var actualResult = await this.fileCabinet.MoveFileAsync(sourceKey, destKey, handleExisting);

            this.mockStorageProvider.Verify(s => s.MoveFileAsync(sourceKey, destKey, handleExisting, mockConfig.Object), Times.Once);

            Assert.Equal(mockResult.Object, actualResult);
        }

        [Theory]
        [InlineData("key", HandleExistingMethod.Overwrite)]
        [InlineData("key", HandleExistingMethod.Skip)]
        [InlineData("key", HandleExistingMethod.Throw)]
        public async Task Save_File(string key, HandleExistingMethod handleExisting) {
            var mockStream = new Mock<Stream>();
            var mockResult = new Mock<ISaveResult>();

            this.mockStorageProvider.Setup(s => s.SaveFileAsync(key, mockStream.Object, handleExisting, mockConfig.Object)).ReturnsAsync(mockResult.Object);

            var actualResult = await this.fileCabinet.SaveFileAsync(key, mockStream.Object, handleExisting);

            this.mockStorageProvider.Verify(s => s.SaveFileAsync(key, mockStream.Object, handleExisting, mockConfig.Object), Times.Once);

            Assert.Equal(mockResult.Object, actualResult);
        }

        [Theory]
        [InlineData("key")]
        public async Task Delete_File(string key) {
            var mockResult = new Mock<IDeleteResult>();

            this.mockStorageProvider.Setup(s => s.DeleteFileAsync(key, mockConfig.Object)).ReturnsAsync(mockResult.Object);

            var actualResult = await this.fileCabinet.DeleteFileAsync(key);

            this.mockStorageProvider.Verify(s => s.DeleteFileAsync(key, mockConfig.Object), Times.Once);

            Assert.Equal(mockResult.Object, actualResult);
        }
    }
}

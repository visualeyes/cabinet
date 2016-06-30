using Cabinet.Core;
using Cabinet.Core.Exceptions;
using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using Cabinet.Migrator;
using Cabinet.Migrator.Replication;
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

namespace Cabinet.Tests.Migrator.Replication {
    public class ReplicatedStorageProviderFacts {
        private const string ValidKey = "key";

        private readonly Mock<IFileCabinetFactory> mockCabinetFactory;
        private readonly Mock<IStorageProviderConfig> mockMasterConfig;
        private readonly Mock<IStorageProviderConfig> mockReplicaConfig;
        private readonly Mock<IFileCabinet> mockMasterCabinet;
        private readonly Mock<IFileCabinet> mockReplicaCabinet;
        private readonly ReplicatedProviderConfig config;
        private readonly ReplicatedStorageProvider provider;

        public ReplicatedStorageProviderFacts() {
            this.mockCabinetFactory = new Mock<IFileCabinetFactory>();
            this.mockMasterConfig = new Mock<IStorageProviderConfig>();
            this.mockReplicaConfig = new Mock<IStorageProviderConfig>();

            this.mockMasterCabinet = new Mock<IFileCabinet>();
            this.mockReplicaCabinet = new Mock<IFileCabinet>();

            this.config = new ReplicatedProviderConfig(mockMasterConfig.Object, mockReplicaConfig.Object);

            this.mockCabinetFactory.Setup(f => f.GetCabinet(mockMasterConfig.Object)).Returns(mockMasterCabinet.Object);
            this.mockCabinetFactory.Setup(f => f.GetCabinet(mockReplicaConfig.Object)).Returns(mockReplicaCabinet.Object);

            this.provider = new ReplicatedStorageProvider(this.mockCabinetFactory.Object);
        }

        [Fact]
        public void Provider_Type() {
            Assert.Equal(ReplicatedProviderConfig.ProviderType, provider.ProviderType);
        }
        
        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Exists_Null_Or_Empty_Key(string key) {
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.ExistsAsync(key, this.config));
        }

        [Fact]
        public async Task Exists_Null_Config() {
            ReplicatedProviderConfig nullConfig = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.ExistsAsync(ValidKey, nullConfig));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Exists(bool expectedExists) {
            string key = ValidKey;

            this.mockMasterCabinet.Setup(c => c.ExistsAsync(key)).ReturnsAsync(expectedExists);

            bool exists = await this.provider.ExistsAsync(key, this.config);

            this.mockMasterCabinet.Verify(c => c.ExistsAsync(key), Times.Once); // Just check master
            this.mockReplicaCabinet.Verify(c => c.ExistsAsync(key), Times.Never);

            Assert.Equal(expectedExists, exists);
        }

        [Fact]
        public async Task List_Keys_Null_Config() {
            ReplicatedProviderConfig nullConfig = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.ListKeysAsync(nullConfig));
        }

        [Theory]
        [InlineData(null, true), InlineData("", true), InlineData("folder", false)]
        public async Task List_Keys(string keyPrefix, bool recursive) {
            var expectedKeys = new string[] { "one", "two", "three" };
            
            this.mockMasterCabinet.Setup(c => c.ListKeysAsync(keyPrefix, recursive)).ReturnsAsync(expectedKeys);

            var keys = await provider.ListKeysAsync(this.config, keyPrefix, recursive);

            this.mockMasterCabinet.Verify(c => c.ListKeysAsync(keyPrefix, recursive), Times.Once);
            this.mockReplicaCabinet.Verify(c => c.ListKeysAsync(keyPrefix, recursive), Times.Never);

            Assert.Equal(expectedKeys, keys);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Get_File_Async_NullKey_Throws(string key) {
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetItemAsync(key, config));
        }

        [Fact]
        public async Task Get_File_Async_NullConfig_Throws() {
            ReplicatedProviderConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetItemAsync("key", config));
        }

        [Theory]
        [InlineData("blah", true)]
        [InlineData("bar", false)]
        public async Task Get_File(string key, bool exists) {
            var exectedItem = new TestCabinetFileInfo(key, exists, ItemType.File);
            
            this.mockMasterCabinet.Setup(c => c.GetItemAsync(key)).ReturnsAsync(exectedItem);

            var item = await provider.GetItemAsync(key, config);

            this.mockMasterCabinet.Verify(c => c.GetItemAsync(key), Times.Once);
            this.mockReplicaCabinet.Verify(c => c.GetItemAsync(key), Times.Never);

            Assert.Equal(exectedItem, item);
        }

        [Theory]
        [InlineData("blah", true)]
        [InlineData("bar", false)]
        public async Task Open_ReadStream_Async(string key, bool exists) {
            var mockStream = new Mock<Stream>();

            if(exists) {
                this.mockMasterCabinet.Setup(c => c.OpenReadStreamAsync(key)).ReturnsAsync(mockStream.Object);
            } else {
                this.mockMasterCabinet.Setup(c => c.OpenReadStreamAsync(key)).ThrowsAsync(new CabinetFileOpenException(key, new Exception("Can't open file!")));
            }

            if(exists) {
                var stream = await provider.OpenReadStreamAsync(key, config);
                Assert.Equal(mockStream.Object, stream);
            } else {
                await Assert.ThrowsAsync<CabinetFileOpenException>(() => provider.OpenReadStreamAsync(key, config));
            }

            this.mockMasterCabinet.Verify(c => c.OpenReadStreamAsync(key), Times.Once);
            this.mockReplicaCabinet.Verify(c => c.OpenReadStreamAsync(key), Times.Never);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Save_File_Path_NullKey_Throws(string key) {
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.SaveFileAsync(key, "C:\test\file.txt", HandleExistingMethod.Overwrite, null, config));
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Save_File_Path_NullPath_Throws(string filePath) {
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.SaveFileAsync("key", filePath, HandleExistingMethod.Overwrite, null, config));
        }

        [Fact]
        public async Task Save_File_Path_NullConfig_Throws() {
            ReplicatedProviderConfig config = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                provider.SaveFileAsync("key", "C:\test\file.txt", HandleExistingMethod.Overwrite, null, config)
            );
        }

        [Theory]
        [InlineData("test", "C:\test\file.txt", HandleExistingMethod.Overwrite, true)]
        [InlineData("test", "C:\test\file.txt", HandleExistingMethod.Skip, true)]
        [InlineData("test", "C:\test\file.txt", HandleExistingMethod.Throw, true)]
        public async Task Save_File_Path(string key, string filePath, HandleExistingMethod handleExisting, bool masterSuccess) {
            var mockMasterSaveResult = new Mock<ISaveResult>();
            var mockReplicaSaveResult = new Mock<ISaveResult>();

            mockMasterSaveResult.SetupGet(r => r.Success).Returns(masterSuccess);

            this.mockMasterCabinet.Setup(c => c.SaveFileAsync(key, filePath, handleExisting, null)).ReturnsAsync(mockMasterSaveResult.Object);
            this.mockReplicaCabinet.Setup(c => c.SaveFileAsync(key, filePath, handleExisting, null)).ReturnsAsync(mockReplicaSaveResult.Object);

            var saveResult = await provider.SaveFileAsync(key, filePath, handleExisting, null, config);

            this.mockMasterCabinet.Verify(c => c.SaveFileAsync(key, filePath, handleExisting, null), Times.Once);
            this.mockReplicaCabinet.Verify(c => c.SaveFileAsync(key, filePath, handleExisting, null), masterSuccess ? Times.Once() : Times.Never());

            Assert.Equal(mockMasterSaveResult.Object, saveResult);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Save_File_Stream_NullKey_Throws(string key) {
            var mockStream = new Mock<Stream>();
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.SaveFileAsync(key, mockStream.Object, HandleExistingMethod.Overwrite, null, config));
        }

        [Fact]
        public async Task Save_File_Stream_NullStream_Throws() {
            Stream stream = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.SaveFileAsync("key", stream, HandleExistingMethod.Overwrite, null, config));
        }

        [Fact]
        public async Task Save_File_Stream_NullConfig_Throws() {
            var mockStream = new Mock<Stream>();
            ReplicatedProviderConfig config = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                provider.SaveFileAsync("key", mockStream.Object, HandleExistingMethod.Overwrite, null, config)
            );
        }

        [Theory]
        [InlineData("test", HandleExistingMethod.Overwrite, true)]
        [InlineData("test", HandleExistingMethod.Skip, true)]
        [InlineData("test", HandleExistingMethod.Throw, true)]
        public async Task Save_File_Path(string key, HandleExistingMethod handleExisting, bool masterSuccess) {
            var mockStream = new Mock<Stream>();
            var mockMasterSaveResult = new Mock<ISaveResult>();
            var mockReplicaSaveResult = new Mock<ISaveResult>();

            mockMasterSaveResult.SetupGet(r => r.Success).Returns(masterSuccess);

            this.mockMasterCabinet.Setup(c => c.SaveFileAsync(key, mockStream.Object, handleExisting, null)).ReturnsAsync(mockMasterSaveResult.Object);
            this.mockReplicaCabinet.Setup(c => c.SaveFileAsync(key, mockStream.Object, handleExisting, null)).ReturnsAsync(mockReplicaSaveResult.Object);

            var saveResult = await provider.SaveFileAsync(key, mockStream.Object, handleExisting, null, config);

            this.mockMasterCabinet.Verify(c => c.SaveFileAsync(key, mockStream.Object, handleExisting, null), Times.Once);
            this.mockReplicaCabinet.Verify(c => c.SaveFileAsync(key, mockStream.Object, handleExisting, null), masterSuccess ? Times.Once() : Times.Never());

            Assert.Equal(mockMasterSaveResult.Object, saveResult);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Delete_File_NullKey_Throws(string key) {
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.DeleteFileAsync(key, config));
        }
        
        [Fact]
        public async Task Delete_File_NullConfig_Throws() {
            ReplicatedProviderConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.DeleteFileAsync("key", config));
        }

        [Theory]
        [InlineData(true), InlineData(false)]
        public async Task Delete_File(bool masterSuccess) {
            string key = "test";

            var mockMasterSaveResult = new Mock<IDeleteResult>();
            mockMasterSaveResult.SetupGet(m => m.Success).Returns(masterSuccess);

            var mockReplicaSaveResult = new Mock<IDeleteResult>();

            this.mockMasterCabinet.Setup(c => c.DeleteFileAsync(key)).ReturnsAsync(mockMasterSaveResult.Object);
            this.mockReplicaCabinet.Setup(c => c.DeleteFileAsync(key)).ReturnsAsync(mockReplicaSaveResult.Object);

            var result = await provider.DeleteFileAsync(key, config);

            this.mockMasterCabinet.Verify(c => c.DeleteFileAsync(key), Times.Once);
            this.mockReplicaCabinet.Verify(c => c.DeleteFileAsync(key), masterSuccess ? Times.Once() : Times.Never());

            Assert.Equal(mockMasterSaveResult.Object, result);
        }
    }
}

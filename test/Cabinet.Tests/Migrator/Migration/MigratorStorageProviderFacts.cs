using Cabinet.Core;
using Cabinet.Core.Exceptions;
using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using Cabinet.Migrator;
using Cabinet.Migrator.Migration;
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

namespace Cabinet.Tests.Migrator.Migration {
    public class MigratorStorageProviderFacts {
        private const string ValidKey = "key";

        private readonly Mock<IFileCabinetFactory> mockCabinetFactory;
        private readonly Mock<IStorageProviderConfig> mockFromConfig;
        private readonly Mock<IStorageProviderConfig> mockToConfig;
        private readonly Mock<IFileCabinet> mockFromCabinet;
        private readonly Mock<IFileCabinet> mockToCabinet;
        private readonly MigrationProviderConfig config;
        private readonly MigrationStorageProvider provider;

        public MigratorStorageProviderFacts() {
            this.mockCabinetFactory = new Mock<IFileCabinetFactory>();
            this.mockFromConfig = new Mock<IStorageProviderConfig>();
            this.mockToConfig = new Mock<IStorageProviderConfig>();

            this.mockFromCabinet = new Mock<IFileCabinet>();
            this.mockToCabinet = new Mock<IFileCabinet>();

            this.config = new MigrationProviderConfig(mockFromConfig.Object, mockToConfig.Object);

            this.mockCabinetFactory.Setup(f => f.GetCabinet(mockFromConfig.Object)).Returns(mockFromCabinet.Object);
            this.mockCabinetFactory.Setup(f => f.GetCabinet(mockToConfig.Object)).Returns(mockToCabinet.Object);

            this.provider = new MigrationStorageProvider(this.mockCabinetFactory.Object);
        }

        [Fact]
        public void Provider_Type() {
            Assert.Equal(MigrationProviderConfig.ProviderType, provider.ProviderType);
        }
        
        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Exists_Null_Or_Empty_Key(string key) {
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.ExistsAsync(key, this.config));
        }

        [Fact]
        public async Task Exists_Null_Config() {
            MigrationProviderConfig nullConfig = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.ExistsAsync(ValidKey, nullConfig));
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, false, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        public async Task Exists_In_To(bool existsInTo, bool existsInFrom, bool expectedExists) {
            string key = ValidKey;

            this.mockToCabinet.Setup(c => c.ExistsAsync(key)).ReturnsAsync(existsInTo);
            this.mockFromCabinet.Setup(c => c.ExistsAsync(key)).ReturnsAsync(existsInFrom);

            bool exists = await this.provider.ExistsAsync(key, this.config);

            var fromTimes = existsInTo ? Times.Never() : Times.Once();

            this.mockToCabinet.Verify(c => c.ExistsAsync(key), Times.Once); // always called
            this.mockFromCabinet.Verify(c => c.ExistsAsync(key), fromTimes); // only called if not in To

            Assert.Equal(expectedExists, exists);
        }

        [Fact]
        public async Task List_Keys_Null_Config() {
            MigrationProviderConfig nullConfig = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.ListKeysAsync(nullConfig));
        }

        [Theory]
        [MemberData("GetListKeysData")]
        public async Task List_Keys(string keyPrefix, bool recursive, IEnumerable<string> toKeys, IEnumerable<string> fromKeys, IEnumerable<string> expectedKeys) {

            this.mockToCabinet.Setup(c => c.ListKeysAsync(keyPrefix, recursive)).ReturnsAsync(toKeys);
            this.mockFromCabinet.Setup(c => c.ListKeysAsync(keyPrefix, recursive)).ReturnsAsync(fromKeys);

            var keys = await provider.ListKeysAsync(this.config);

            this.mockToCabinet.Verify(c => c.ListKeysAsync(keyPrefix, recursive), Times.Once);
            this.mockFromCabinet.Verify(c => c.ListKeysAsync(keyPrefix, recursive), Times.Once);

            Assert.Equal(expectedKeys, keys);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Get_File_Async_NullKey_Throws(string key) {
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetFileAsync(key, config));
        }

        [Fact]
        public async Task Get_File_Async_NullConfig_Throws() {
            MigrationProviderConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetFileAsync("key", config));
        }

        [Theory]
        [InlineData("blah", true, true), InlineData("foo", true, false)]
        [InlineData("bar", false, true), InlineData("baz", false, false)]
        public async Task Get_File(string key, bool existsInTo, bool existsInFrom) {
            var toItem = new TestCabinetFileInfo(key, existsInTo, ItemType.File);
            var fromItem = new TestCabinetFileInfo(key, existsInFrom, ItemType.File);

            this.mockToCabinet.Setup(c => c.GetItemAsync(key)).ReturnsAsync(toItem);
            this.mockFromCabinet.Setup(c => c.GetItemAsync(key)).ReturnsAsync(fromItem);

            var item = await provider.GetFileAsync(key, config);

            this.mockToCabinet.Verify(c => c.GetItemAsync(key), Times.Once);
            this.mockFromCabinet.Verify(c => c.GetItemAsync(key), existsInTo ? Times.Never() : Times.Once());

            if(existsInTo) {
                Assert.Equal(toItem, item);
            } else {
                Assert.Equal(fromItem, item);
            }
        }

        [Theory]
        [InlineData("blah", true, true), InlineData("foo", true, false)]
        [InlineData("bar", false, true), InlineData("baz", false, false)]
        public async Task Open_ReadStream_Async(string key, bool existsInTo, bool existsInFrom) {
            var mockToStream = new Mock<Stream>();
            var mockFromStream = new Mock<Stream>();

            SetupOpenReadStream(this.mockToCabinet, key, existsInTo, mockToStream);
            SetupOpenReadStream(this.mockFromCabinet, key, existsInFrom, mockFromStream);

            if(existsInTo || existsInFrom) {
                var stream = await provider.OpenReadStreamAsync(key, config);

                if(existsInTo) {
                    Assert.Equal(mockToStream.Object, stream);
                } else {
                    Assert.Equal(mockFromStream.Object, stream);
                }
            } else {
                await Assert.ThrowsAsync<CabinetFileOpenException>(() => provider.OpenReadStreamAsync(key, config));
            }

            this.mockToCabinet.Verify(c => c.OpenReadStreamAsync(key), Times.Once);
            this.mockFromCabinet.Verify(c => c.OpenReadStreamAsync(key), existsInTo ? Times.Never() : Times.Once());
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
            MigrationProviderConfig config = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                provider.SaveFileAsync("key", "C:\test\file.txt", HandleExistingMethod.Overwrite, null, config)
            );
        }
        [Theory]
        [InlineData("test", "C:\test\file.txt", HandleExistingMethod.Overwrite)]
        [InlineData("test", "C:\test\file.txt", HandleExistingMethod.Skip)]
        [InlineData("test", "C:\test\file.txt", HandleExistingMethod.Throw)]
        public async Task Save_File_Path(string key, string filePath, HandleExistingMethod handleExisting) {
            var mockSaveResult = new Mock<ISaveResult>();

            this.mockToCabinet.Setup(c => c.SaveFileAsync(key, filePath, handleExisting, null)).ReturnsAsync(mockSaveResult.Object);

            await provider.SaveFileAsync(key, filePath, handleExisting, null, config);

            this.mockToCabinet.Verify(c => c.SaveFileAsync(key, filePath, handleExisting, null), Times.Once);
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
            MigrationProviderConfig config = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                provider.SaveFileAsync("key", mockStream.Object, HandleExistingMethod.Overwrite, null, config)
            );
        }
        [Theory]
        [InlineData("test", HandleExistingMethod.Overwrite)]
        [InlineData("test", HandleExistingMethod.Skip)]
        [InlineData("test", HandleExistingMethod.Throw)]
        public async Task Save_File_Stream(string key, HandleExistingMethod handleExisting) {
            var mockStream = new Mock<Stream>();
            var mockSaveResult = new Mock<ISaveResult>();

            this.mockToCabinet.Setup(c => c.SaveFileAsync(key, mockStream.Object, handleExisting, null)).ReturnsAsync(mockSaveResult.Object);

            await provider.SaveFileAsync(key, mockStream.Object, handleExisting, null, config);

            this.mockToCabinet.Verify(c => c.SaveFileAsync(key, mockStream.Object, handleExisting, null), Times.Once);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Delete_File_NullKey_Throws(string key) {
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.DeleteFileAsync(key, config));
        }
        
        [Fact]
        public async Task Delete_File_NullConfig_Throws() {
            MigrationProviderConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.DeleteFileAsync("key", config));
        }

        [Fact]
        public async Task Delete_File() {
            string key = "test";

            var mockSaveResult = new Mock<IDeleteResult>();

            this.mockToCabinet.Setup(c => c.DeleteFileAsync(key)).ReturnsAsync(mockSaveResult.Object);
            this.mockFromCabinet.Setup(c => c.DeleteFileAsync(key)).ReturnsAsync(mockSaveResult.Object);

            await provider.DeleteFileAsync(key, config);

            this.mockToCabinet.Verify(c => c.DeleteFileAsync(key), Times.Once);
            this.mockFromCabinet.Verify(c => c.DeleteFileAsync(key), Times.Once);
        }

        private static void SetupOpenReadStream(Mock<IFileCabinet> mockCabinet, string key, bool existsInTo, Mock<Stream> mockStream) {
            if(existsInTo) {
                mockCabinet.Setup(c => c.OpenReadStreamAsync(key)).ReturnsAsync(mockStream.Object);
            } else {
                mockCabinet.Setup(c => c.OpenReadStreamAsync(key)).ThrowsAsync(new CabinetFileOpenException(key, new Exception("Can't open file!")));
            }
        }

        public static object[] GetListKeysData() {
            return new object[] {
                new object[] { "", true, new string[] { }, new string[] { }, new string[] { } },
                new object[] { "", true, new string[] { "one", "two", "three" }, new string[] { }, new string[] { "one", "two", "three" } },
                new object[] { "", true, new string[] { }, new string[] { "one", "two", "three" }, new string[] { "one", "two", "three" } },
                new object[] { "", true, new string[] { "one", "two" }, new string[] { "three" }, new string[] { "one", "two", "three" } },
                new object[] { "", true, new string[] { "one", "two" }, new string[] { "two", "three" }, new string[] { "one", "two", "three" } }
            };
        }
    }
}

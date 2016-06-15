using Cabinet.Core;
using Cabinet.Core.Providers;
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

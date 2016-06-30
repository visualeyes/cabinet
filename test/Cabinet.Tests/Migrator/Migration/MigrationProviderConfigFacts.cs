using Cabinet.Core.Providers;
using Cabinet.Migrator.Migration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Migrator.Replication {
    public class MigrationProviderConfigFacts {

        [Fact]
        public void Null_Master_Throws() {
            var from = new Mock<IStorageProviderConfig>();
            IStorageProviderConfig to = null;

            Assert.Throws<ArgumentNullException>(() => new MigrationProviderConfig(from.Object, to));
        }

        [Fact]
        public void Null_Replica_Throws() {
            IStorageProviderConfig from = null;
            var to = new Mock<IStorageProviderConfig>();

            Assert.Throws<ArgumentNullException>(() => new MigrationProviderConfig(from, to.Object));
        }

        [Fact]
        public void From_To_Config_Set() {
            var from = new Mock<IStorageProviderConfig>();
            var to = new Mock<IStorageProviderConfig>();

            var config = new MigrationProviderConfig(from.Object, to.Object);
            
            Assert.Equal(from.Object, config.From);
            Assert.Equal(to.Object, config.To);
        }

        [Fact]
        public void Delimiter_DefaultConfig_Set() {
            var from = new Mock<IStorageProviderConfig>();
            var to = new Mock<IStorageProviderConfig>();

            from.SetupGet(f => f.Delimiter).Returns("/");

            var config = new MigrationProviderConfig(from.Object, to.Object);

            Assert.Equal(from.Object, config.From);
            Assert.Equal(to.Object, config.To);
            Assert.Equal(from.Object.Delimiter, config.Delimiter);
        }

        [Fact]
        public void Delimiter_Config_Set() {
            string delimiter = "/";
            var from = new Mock<IStorageProviderConfig>();
            var to = new Mock<IStorageProviderConfig>();

            var config = new MigrationProviderConfig(from.Object, to.Object, delimiter);

            Assert.Equal(from.Object, config.From);
            Assert.Equal(to.Object, config.To);
            Assert.Equal(delimiter, config.Delimiter);
        }
    }
}

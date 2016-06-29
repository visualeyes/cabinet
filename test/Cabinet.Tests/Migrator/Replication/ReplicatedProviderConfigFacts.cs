using Cabinet.Core.Providers;
using Cabinet.Migrator.Replication;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Migrator.Replication {
    public class ReplicatedProviderConfigFacts {

        [Fact]
        public void Null_Master_Throws() {
            var master = new Mock<IStorageProviderConfig>();
            IStorageProviderConfig replica = null;

            Assert.Throws<ArgumentNullException>(() => new ReplicatedProviderConfig(master.Object, replica));
        }

        [Fact]
        public void Null_Replica_Throws() {
            IStorageProviderConfig master = null;
            var replica = new Mock<IStorageProviderConfig>();

            Assert.Throws<ArgumentNullException>(() => new ReplicatedProviderConfig(master, replica.Object));
        }

        [Fact]
        public void Config_Set() {
            var master = new Mock<IStorageProviderConfig>();
            var replica = new Mock<IStorageProviderConfig>();

            var config = new ReplicatedProviderConfig(master.Object, replica.Object);
            
            Assert.Equal(master.Object, config.Master);
            Assert.Equal(replica.Object, config.Replica);
        }
    }
}

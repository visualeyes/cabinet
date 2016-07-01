using Cabinet.Core;
using Cabinet.Migrator;
using Cabinet.Migrator.Replication;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Migrator.Replication {
    public class CabinetReplicationTaskFacts {
        private readonly Mock<ICabinetFileReplicator> mockCabinetFileReplicator;
        private readonly CabinetReplicationTask cabinetReplicatorTask;
        private readonly Mock<IMigrationTaskRunner> mockTaskRunner;

        public CabinetReplicationTaskFacts() {
            this.mockTaskRunner = new Mock<IMigrationTaskRunner>();
            this.mockCabinetFileReplicator = new Mock<ICabinetFileReplicator>();
            this.cabinetReplicatorTask = new CabinetReplicationTask(mockTaskRunner.Object, mockCabinetFileReplicator.Object);
        }

        [Fact]
        public async Task ReplicateCabinetAsync() {
            var keys = new List<string>() {
                "one", "two",
                "bar/one", "bar/two"
            };

            var mockMasterCabinet = new Mock<IFileCabinet>();
            var mockReplicaCabinet = new Mock<IFileCabinet>();

            mockMasterCabinet
                .Setup(c => c.ListKeysAsync("", true))
                .ReturnsAsync(keys);

            mockTaskRunner
                .Setup(r => r.RunTasks(It.IsAny<Func<string, Task>>(), keys, default(CancellationToken)))
                .Returns(Task.FromResult(0));
                
            await cabinetReplicatorTask.ReplicateCabinetAsync(mockMasterCabinet.Object, mockReplicaCabinet.Object, CancellationToken.None);

            mockMasterCabinet.Verify(c => c.ListKeysAsync("", true), Times.Once);
            mockTaskRunner.Verify(r => r.RunTasks(It.IsAny<Func<string, Task>>(), keys, default(CancellationToken)), Times.Once);
        }
    }
}

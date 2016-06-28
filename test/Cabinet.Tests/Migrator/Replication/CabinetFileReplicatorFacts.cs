using Cabinet.Core;
using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using Cabinet.Migrator;
using Cabinet.Migrator.Replication;
using Cabinet.Tests.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Migrator.Replication {
    public class CabinetFileReplicatorFacts {
        private const string ReplicaKey = "replica";

        private readonly CabinetFileReplicator cabinetReplicator;

        public CabinetFileReplicatorFacts() {
            this.cabinetReplicator = new CabinetFileReplicator();
        }
        
        [Theory]
        [InlineData(false, null, false, null, ReplicationFileState.Same)]
        [InlineData(false, null, true, "2016-06-28 10:00:00", ReplicationFileState.SourceDeleted)]
        [InlineData(true, "2016-06-28 10:00:00", false, null, ReplicationFileState.SourceAdded)]
        [InlineData(true, "2016-06-28 10:00:00", true, "2016-06-28 08:00:00", ReplicationFileState.SourceNewer)]
        [InlineData(true, "2016-06-28 08:00:00", true, "2016-06-28 10:00:00", ReplicationFileState.ReplicationNewer)]
        [InlineData(true, "2016-06-28 10:00:00", true, "2016-06-28 10:00:00", ReplicationFileState.ReplicationNewer)]
        public void GetReplicationAction(bool sourceExists, string sourceModified, bool destExists, string destModified, ReplicationFileState expectedState) {
            var source = CreateTestFileInfo(sourceExists, sourceModified);
            var dest = CreateTestFileInfo(destExists, destModified);

            var actualState = this.cabinetReplicator.GetReplicationFileState(source, dest);

            Assert.Equal(expectedState, actualState);
        }

        [Theory]
        [InlineData(false, null, false, null, ReplicationFileState.Same)]
        [InlineData(false, null, true, "2016-06-28 10:00:00", ReplicationFileState.SourceDeleted)]
        [InlineData(true, "2016-06-28 10:00:00", false, null, ReplicationFileState.SourceAdded)]
        [InlineData(true, "2016-06-28 10:00:00", true, "2016-06-28 08:00:00", ReplicationFileState.SourceNewer)]
        [InlineData(true, "2016-06-28 08:00:00", true, "2016-06-28 10:00:00", ReplicationFileState.ReplicationNewer)]
        [InlineData(true, "2016-06-28 10:00:00", true, "2016-06-28 10:00:00", ReplicationFileState.ReplicationNewer)]
        public async Task ReplicateKey(bool sourceExists, string sourceModified, bool destExists, string destModified, ReplicationFileState expectedState) {
            var source = CreateTestFileInfo(sourceExists, sourceModified);
            var dest = CreateTestFileInfo(destExists, destModified);

            var mockMasterCabinet = new Mock<IFileCabinet>();
            var mockReplicaCabinet = new Mock<IFileCabinet>();
            var mockStream = new Mock<Stream>();
            var mockSaveResult = new Mock<ISaveResult>();

            mockMasterCabinet
                .Setup(m => m.GetItemAsync(ReplicaKey))
                .ReturnsAsync(source);

            mockReplicaCabinet
                .Setup(c => c.GetItemAsync(ReplicaKey))
                .ReturnsAsync(dest);

            mockMasterCabinet
                .Setup(m => m.OpenReadStreamAsync(ReplicaKey))
                .ReturnsAsync(mockStream.Object);

            mockReplicaCabinet
                .Setup(c => c.SaveFileAsync(ReplicaKey, mockStream.Object, HandleExistingMethod.Overwrite, null))
                .ReturnsAsync(mockSaveResult.Object);

            await this.cabinetReplicator.ReplicateKeyAsync(ReplicaKey, mockMasterCabinet.Object, mockReplicaCabinet.Object);

            switch(expectedState) {
                case ReplicationFileState.SourceAdded:
                case ReplicationFileState.SourceNewer:
                    mockMasterCabinet.Verify(c => c.OpenReadStreamAsync(ReplicaKey), Times.Once);
                    mockReplicaCabinet.Verify(c => c.SaveFileAsync(ReplicaKey, mockStream.Object, HandleExistingMethod.Overwrite, null), Times.Once);
                    break;
                case ReplicationFileState.SourceDeleted:
                    mockReplicaCabinet.Verify(c => c.DeleteFileAsync(ReplicaKey), Times.Once);
                    break;
                case ReplicationFileState.ReplicationNewer:
                case ReplicationFileState.Same:
                default:
                    mockMasterCabinet.Verify(c => c.OpenReadStreamAsync(ReplicaKey), Times.Never);
                    mockReplicaCabinet.Verify(c => c.SaveFileAsync(ReplicaKey, mockStream.Object, HandleExistingMethod.Overwrite, null), Times.Never);
                    mockReplicaCabinet.Verify(c => c.DeleteFileAsync(ReplicaKey), Times.Never);
                    break;
            }
        }

        private static TestCabinetFileInfo CreateTestFileInfo(bool destExists, string destModified) {
            return new TestCabinetFileInfo(ReplicaKey, destExists, ItemType.File) {
                LastModifiedUtc = String.IsNullOrWhiteSpace(destModified) ? null : (DateTime?)DateTime.Parse(destModified)
            };
        }
    }
}

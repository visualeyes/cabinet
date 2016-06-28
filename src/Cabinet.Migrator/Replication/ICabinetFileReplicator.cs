using System.Threading.Tasks;
using Cabinet.Core;

namespace Cabinet.Migrator.Replication {
    public interface ICabinetFileReplicator {
        ReplicationFileState GetReplicationFileState(ICabinetItemInfo sourceFile, ICabinetItemInfo destFile);
        Task ReplicateKeyAsync(string key, IFileCabinet masterCabinet, IFileCabinet replicaCabinet);
    }
}
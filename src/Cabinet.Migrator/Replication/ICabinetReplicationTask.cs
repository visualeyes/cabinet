using System.Threading;
using System.Threading.Tasks;
using Cabinet.Core;

namespace Cabinet.Migrator.Replication {
    public interface ICabinetReplicationTask {
        Task ReplicateCabinetAsync(IFileCabinet masterCabinet, IFileCabinet replicaCabinet, CancellationToken cancellationToken);
    }
}
using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cabinet.Migrator.Replication {
    public class CabinetReplicationTask : ICabinetReplicationTask {
        private readonly IMigrationTaskRunner taskRunner;
        private readonly ICabinetFileReplicator cabinetReplicator;

        public CabinetReplicationTask(IMigrationTaskRunner taskRunner, ICabinetFileReplicator cabinetReplicator) {
            this.taskRunner = taskRunner;
            this.cabinetReplicator = cabinetReplicator;
        }

        /// <summary>
        /// Syncs the Master Cabinet to the Replica Cabinet
        /// </summary>
        /// <param name="masterCabinet"></param>
        /// <param name="replicaCabinet"></param>
        /// <returns></returns>
        public async Task ReplicateCabinetAsync(IFileCabinet masterCabinet, IFileCabinet replicaCabinet, CancellationToken cancellationToken) {
            // Just get they keys here - by the time the item is processed it may have changed
            var sourceKeys = await masterCabinet.ListKeysAsync(recursive: true);

            if(!sourceKeys.Any()) return;

            await taskRunner.RunTasks(async (key) => {
                await cabinetReplicator.ReplicateKeyAsync(key, masterCabinet, replicaCabinet);
                return key;
            }, sourceKeys, cancellationToken);
        }
    }
}

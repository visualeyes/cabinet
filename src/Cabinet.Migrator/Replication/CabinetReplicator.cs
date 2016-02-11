using Cabinet.Core;
using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Cabinet.Migrator.Replication {
    public class CabinetReplicator {
        private readonly IMigrationTaskRunner taskRunner;
        private readonly CabinetItemInfoKeyComparer comparer;

        public CabinetReplicator(IMigrationTaskRunner taskRunner) {
            this.taskRunner = taskRunner;
            this.comparer = new CabinetItemInfoKeyComparer();
        }

        /// <summary>
        /// Syncs the Master Cabinet to the Replica Cabinet
        /// </summary>
        /// <param name="masterCabinet"></param>
        /// <param name="replicaCabinet"></param>
        /// <returns></returns>
        public async Task ReplicateCabinet(IFileCabinet masterCabinet, IFileCabinet replicaCabinet, CancellationToken cancellationToken) {
            // Just get they keys here - by the time the item is processed it may have changed
            var sourceKeys = await masterCabinet.ListKeysAsync(recursive: true);

            await taskRunner.RunTasks(async (key) => {
                var sourceFile = await masterCabinet.GetItemAsync(key);
                var destFile = await replicaCabinet.GetItemAsync(key);

                var replicationAction = GetReplicationAction(sourceFile, destFile);

                switch(replicationAction) {
                    case ReplicationFileState.SourceAdded:
                        await SaveFileAsync(masterCabinet, replicaCabinet, key, HandleExistingMethod.Throw);
                        break;
                    case ReplicationFileState.SourceNewer:
                        await SaveFileAsync(masterCabinet, replicaCabinet, key, HandleExistingMethod.Overwrite);
                        break;
                    case ReplicationFileState.ReplicationNewer:
                        // if the replication is newer it's aready been synced
                        break;
                    case ReplicationFileState.SourceDeleted:
                        await replicaCabinet.DeleteFileAsync(key);
                        break;
                    case ReplicationFileState.Same:
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return 1;
            }, sourceKeys, cancellationToken);
        }

        private static async Task SaveFileAsync(IFileCabinet masterCabinet, IFileCabinet replicationCabinet, string key, HandleExistingMethod handleExisting) {
            using (var stream = await masterCabinet.OpenReadStreamAsync(key)) {
                await replicationCabinet.SaveFileAsync(key, stream, handleExisting);
            }
        }

        private ReplicationFileState GetReplicationAction(ICabinetItemInfo sourceFile, ICabinetItemInfo destFile) {
            if(!sourceFile.Exists) {
                return destFile.Exists ? ReplicationFileState.SourceDeleted : ReplicationFileState.Same;
            }

            if(!destFile.Exists) {
                return ReplicationFileState.SourceAdded;
            }

            return sourceFile.LastModifiedUtc > destFile.LastModifiedUtc ? ReplicationFileState.SourceNewer : ReplicationFileState.ReplicationNewer;

        }
    }
}

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
    public class CabinetFileReplicator : ICabinetFileReplicator {

        public async Task ReplicateKeyAsync(string key, IFileCabinet masterCabinet, IFileCabinet replicaCabinet) {
            var sourceFileTask = masterCabinet.GetItemAsync(key);
            var destFileTask = replicaCabinet.GetItemAsync(key);

            await Task.WhenAll(sourceFileTask, destFileTask); // Get items simultaneously 
            
            var replicationAction = GetReplicationFileState(sourceFileTask.Result, destFileTask.Result);

            switch(replicationAction) {
                case ReplicationFileState.SourceAdded:
                case ReplicationFileState.SourceNewer:
                    await SaveFileAsync(masterCabinet, replicaCabinet, key);
                    break;
                case ReplicationFileState.SourceDeleted:
                    await replicaCabinet.DeleteFileAsync(key);
                    break;
                case ReplicationFileState.ReplicationNewer: // if the replication is newer it's aready been synced
                case ReplicationFileState.Same:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public ReplicationFileState GetReplicationFileState(ICabinetItemInfo sourceFile, ICabinetItemInfo destFile) {
            if(!sourceFile.Exists) {
                return destFile.Exists ? ReplicationFileState.SourceDeleted : ReplicationFileState.Same;
            }

            if(!destFile.Exists) {
                return ReplicationFileState.SourceAdded;
            }

            return sourceFile.LastModifiedUtc > destFile.LastModifiedUtc ? ReplicationFileState.SourceNewer : ReplicationFileState.ReplicationNewer;
        }

        private static async Task<ISaveResult> SaveFileAsync(IFileCabinet masterCabinet, IFileCabinet replicationCabinet, string key) {
            using(var stream = await masterCabinet.OpenReadStreamAsync(key)) {
                return await replicationCabinet.SaveFileAsync(key, stream, HandleExistingMethod.Overwrite);
            }
        }
    }
}

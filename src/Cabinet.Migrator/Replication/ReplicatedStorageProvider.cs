using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using System.IO;
using Cabinet.Migrator.Results;

namespace Cabinet.Migrator.Replication {
    /// <summary>
    /// Replicated provider to provide for faster replication of files
    ///  -- should be used in conjunction with the <see cref="CabinetFileReplicator" />
    /// </summary>
    public class ReplicatedStorageProvider : IStorageProvider<ReaplicatedProviderConfig> {
        private readonly IFileCabinetFactory cabinetFactory;

        public string ProviderType  => ReaplicatedProviderConfig.ProviderType;

        public ReplicatedStorageProvider(IFileCabinetFactory cabinetFactory) {
            this.cabinetFactory = cabinetFactory;
        }

        public async Task<bool> ExistsAsync(string key, ReaplicatedProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));

            var master = GetMasterConfig(config);
            bool exists = await master.ExistsAsync(key);

            return exists;
        }

        public async Task<IEnumerable<string>> ListKeysAsync(ReaplicatedProviderConfig config, string keyPrefix = "", bool recursive = true) {
            Contract.NotNull(config, nameof(config));

            var master = GetMasterConfig(config);
            var keys = await master.ListKeysAsync(keyPrefix: keyPrefix, recursive: recursive);

            return keys;
        }

        public async Task<ICabinetItemInfo> GetFileAsync(string key, ReaplicatedProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));

            var master = GetMasterConfig(config);
            var item = await master.GetItemAsync(key);

            return item;
        }

        public async Task<IEnumerable<ICabinetItemInfo>> GetItemsAsync(ReaplicatedProviderConfig config, string keyPrefix = "", bool recursive = true) {
            Contract.NotNull(config, nameof(config));

            var master = GetMasterConfig(config);
            var files = await master.GetItemAsync(keyPrefix: keyPrefix, recursive: recursive);

            return files;
        }

        public async Task<Stream> OpenReadStreamAsync(string key, ReaplicatedProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));

            var master = GetMasterConfig(config);
            var stream = await master.OpenReadStreamAsync(key);

            return stream;
        }

        public async Task<ISaveResult> SaveFileAsync(string key, string filePath, HandleExistingMethod handleExisting, IProgress<IWriteProgress> progress, ReaplicatedProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNullOrEmpty(filePath, nameof(filePath));
            Contract.NotNull(config, nameof(config));
            
            return await SaveReplica(async (cabinet) => {
                return await cabinet.SaveFileAsync(key, filePath, handleExisting, progress);
            }, config);
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<IWriteProgress> progress, ReaplicatedProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(content, nameof(content));
            Contract.NotNull(config, nameof(config));

            return await SaveReplica(async (cabinet) => {
                return await cabinet.SaveFileAsync(key, content, handleExisting, progress);
            }, config);
        }

        public async Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting, ReaplicatedProviderConfig config) {
            Contract.NotNullOrEmpty(sourceKey, nameof(sourceKey));
            Contract.NotNullOrEmpty(destKey, nameof(destKey));
            Contract.NotNull(config, nameof(config));

            var master = GetMasterConfig(config);

            var masterResult = await master.MoveFileAsync(sourceKey, destKey, handleExisting);

            if (!masterResult.Success) {
                return masterResult;
            }

            var replica = GetReplicaConfig(config);

            // Ignore result, this should be used with the CabinetReplicator which will handle failures 
            await replica.MoveFileAsync(sourceKey, destKey, handleExisting);

            return masterResult;
        }

        public async Task<IDeleteResult> DeleteFileAsync(string key, ReaplicatedProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));

            var master = GetMasterConfig(config);

            var masterResult = await master.DeleteFileAsync(key);

            if (!masterResult.Success) {
                return masterResult;
            }

            var replica = GetReplicaConfig(config);

            // Ignore result, this should be used with the CabinetReplicator which will handle failures 
            await replica.DeleteFileAsync(key);

            return masterResult;
        }

        private IFileCabinet GetMasterConfig(ReaplicatedProviderConfig config) {
            return GetCabinet(config.Master, "master");
        }

        private IFileCabinet GetReplicaConfig(ReaplicatedProviderConfig config) {
            return GetCabinet(config.Replica, "replica");
        }

        private IFileCabinet GetCabinet(IStorageProviderConfig config, string name) {
            var cabinet = cabinetFactory.GetCabinet(config);

            if (cabinet == null) {
                throw new ApplicationException(String.Format("Could not get cabinet for '{0}' config", name));
            }

            return cabinet;
        }

        private async Task<ISaveResult> SaveReplica(Func<IFileCabinet, Task<ISaveResult>> saveTask, ReaplicatedProviderConfig config) {
            var master = GetMasterConfig(config);

            var masterResult = await saveTask(master);

            if (!masterResult.Success) {
                return masterResult;
            }

            var replica = GetReplicaConfig(config);

            // Ignore result, this should be used with the CabinetReplicator which will handle it 
            await saveTask(replica);

            return masterResult;
        }
    }
}

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
using Cabinet.Core.Exceptions;

namespace Cabinet.Migrator.Migration {
    /// <summary>
    /// The migration provider aims to move files from one provider to another provider
    /// Files are accessed and saved to the 'To' Cabinet
    /// Files are deleted from the 'From' Cabinet after they have been moved
    /// </summary>
    public class MigrationStorageProvider : IStorageProvider<MigrationProviderConfig> {
        private readonly IFileCabinetFactory cabinetFactory;

        public string ProviderType => MigrationProviderConfig.ProviderType;

        public MigrationStorageProvider(IFileCabinetFactory cabinetFactory) {
            this.cabinetFactory = cabinetFactory;
        }

        public async Task<bool> ExistsAsync(string key, MigrationProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));

            var to = GetToCabinet(config);

            bool exists = await to.ExistsAsync(key);

            if (!exists) {
                var from = GetFromCabinet(config);
                exists = await from.ExistsAsync(key); // might not be migrated yet
            }

            return exists;
        }

        public async Task<IEnumerable<string>> ListKeysAsync(MigrationProviderConfig config, string keyPrefix = "", bool recursive = true) {
            Contract.NotNull(config, nameof(config));

            var from = GetFromCabinet(config);
            var to = GetToCabinet(config);

            var keyLists = await Task.WhenAll(
                to.ListKeysAsync(keyPrefix, recursive),
                from.ListKeysAsync(keyPrefix, recursive)
            );

            var keys = new HashSet<string>();

            foreach (var list in keyLists) {
                keys.UnionWith(list);
            }

            return keys;
        }

        public async Task<ICabinetItemInfo> GetFileAsync(string key, MigrationProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));

            var to = GetToCabinet(config);

            var item = await to.GetItemAsync(key);

            if (!item.Exists) {
                var from = GetFromCabinet(config);
                item = await from.GetItemAsync(key);
            }

            return item;
        }

        public Task<IEnumerable<ICabinetItemInfo>> GetItemsAsync(MigrationProviderConfig config, string keyPrefix = "", bool recursive = true) {
            Contract.NotNull(config, nameof(config));

            throw new NotImplementedException();
        }

        public async Task<Stream> OpenReadStreamAsync(string key, MigrationProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));
            if (key == null) throw new ArgumentNullException(nameof(key));

            try {
                var to = GetToCabinet(config);
                return await to.OpenReadStreamAsync(key);
            } catch(CabinetFileOpenException) {
                var from = GetFromCabinet(config);
                return await from.OpenReadStreamAsync(key);
            }
        }

        public async Task<ISaveResult> SaveFileAsync(string key, string filePath, HandleExistingMethod handleExisting, IProgress<IWriteProgress> progress, MigrationProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNullOrEmpty(filePath, nameof(filePath));
            Contract.NotNull(config, nameof(config));
            
            var to = GetToCabinet(config);

            return await to.SaveFileAsync(key, filePath, handleExisting, progress);
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<IWriteProgress> progress, MigrationProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(content, nameof(content));
            Contract.NotNull(config, nameof(config));

            var from = GetFromCabinet(config);
            var to = GetToCabinet(config);

            return await to.SaveFileAsync(key, content, handleExisting, progress);
        }

        public async Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting, MigrationProviderConfig config) {
            Contract.NotNullOrEmpty(sourceKey, nameof(sourceKey));
            Contract.NotNullOrEmpty(destKey, nameof(destKey));
            Contract.NotNull(config, nameof(config));

            var from = GetFromCabinet(config);
            var to = GetToCabinet(config);

            bool sourceExistsInTo = await to.ExistsAsync(sourceKey);

            if (sourceExistsInTo) {
                return await to.MoveFileAsync(sourceKey, destKey, handleExisting);
            }
            
            var fromFile = await from.GetItemAsync(sourceKey);

            if (!fromFile.Exists) {
                return new MoveResult(sourceKey, destKey, false, errorMsg: "Source file does not exist");
            }

            using (var stream = await from.OpenReadStreamAsync(fromFile.Key)) {
                var saveResult = await to.SaveFileAsync(destKey, stream, handleExisting);

                return new MoveResult(
                    sourceKey, destKey,
                    success: saveResult.Success,
                    errorMsg: saveResult.GetErrorMessage()
                );
            }
        }

        public async Task<IDeleteResult> DeleteFileAsync(string key, MigrationProviderConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));

            var from = GetFromCabinet(config);
            var to = GetToCabinet(config);

            var fromResultTask = from.DeleteFileAsync(key);
            var toResultTask = to.DeleteFileAsync(key);

            await Task.WhenAll(fromResultTask, toResultTask);

            return new DeleteResult(fromResultTask.Result, toResultTask.Result);
        }

        private IFileCabinet GetFromCabinet(MigrationProviderConfig config) {
            return GetCabinet(config.From, "from");
        }

        private IFileCabinet GetToCabinet(MigrationProviderConfig config) {
            return GetCabinet(config.To, "to");
        }

        private IFileCabinet GetCabinet(IStorageProviderConfig config, string name) {
            var cabinet = cabinetFactory.GetCabinet(config);

            if (cabinet == null) {
                throw new ApplicationException(String.Format("Could not get cabinet for '{0}' config", name));
            }

            return cabinet;
        }
    }
}

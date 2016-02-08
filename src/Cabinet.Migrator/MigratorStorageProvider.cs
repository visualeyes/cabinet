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

namespace Cabinet.Migrator {
    public class MigratorStorageProvider : IStorageProvider<MigratorProviderConfig> {
        private readonly IFileCabinetFactory cabinetFactory;

        public string ProviderType {
            get { return MigratorProviderConfig.ProviderType; }
        }

        public MigratorStorageProvider(IFileCabinetFactory cabinetFactory) {
            this.cabinetFactory = cabinetFactory;
        }

        public bool SupportsProviderType(string providerType) {
            return true; // TODO: Consider what this should return
        }

        public async Task<bool> ExistsAsync(string key, MigratorProviderConfig config) {
            var from = cabinetFactory.GetCabinet(config.FromConfig);
            var to = cabinetFactory.GetCabinet(config.ToConfig);

            bool exists = await to.ExistsAsync(key);

            if (!exists) {
                exists = await from.ExistsAsync(key); // might not be migrated yet
            }

            return exists;
        }

        public async Task<IEnumerable<string>> ListKeysAsync(MigratorProviderConfig config, string keyPrefix = "", bool recursive = true) {
            var from = cabinetFactory.GetCabinet(config.FromConfig);
            var to = cabinetFactory.GetCabinet(config.ToConfig);

            var keyLists = await Task.WhenAll(
                to.ListKeysAsync(keyPrefix, recursive),
                from.ListKeysAsync(keyPrefix, recursive)
            );

            var keys = new HashSet<string>();

            foreach (var key in keyLists.SelectMany(k => k)) {
                keys.Add(key);
            }

            return keys;
        }

        public async Task<ICabinetItemInfo> GetFileAsync(string key, MigratorProviderConfig config) {
            var from = cabinetFactory.GetCabinet(config.FromConfig);
            var to = cabinetFactory.GetCabinet(config.ToConfig);

            var item = await to.GetFileAsync(key);

            if (!item.Exists) {
                item = await from.GetFileAsync(key);
            }

            return item;
        }

        public Task<IEnumerable<ICabinetItemInfo>> GetItemsAsync(MigratorProviderConfig config, string keyPrefix = "", bool recursive = true) {
            throw new NotImplementedException();
        }

        public async Task<Stream> OpenReadStreamAsync(string key, MigratorProviderConfig config) {
            var from = cabinetFactory.GetCabinet(config.FromConfig);
            var to = cabinetFactory.GetCabinet(config.ToConfig);

            if (key == null) throw new ArgumentNullException(nameof(key));

            var stream = await CheckExistsThenOpenAsync(to, key);
            if (stream == null) {
                stream = await CheckExistsThenOpenAsync(from, key);
            }

            return stream;
        }

        public async Task<ISaveResult> SaveFileAsync(string key, string filePath, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress, MigratorProviderConfig config) {
            var from = cabinetFactory.GetCabinet(config.FromConfig);
            var to = cabinetFactory.GetCabinet(config.ToConfig);

            return await to.SaveFileAsync(key, filePath, handleExisting, progress);
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress, MigratorProviderConfig config) {
            var from = cabinetFactory.GetCabinet(config.FromConfig);
            var to = cabinetFactory.GetCabinet(config.ToConfig);

            return await to.SaveFileAsync(key, content, handleExisting, progress);
        }

        public async Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting, MigratorProviderConfig config) {
            var from = cabinetFactory.GetCabinet(config.FromConfig);
            var to = cabinetFactory.GetCabinet(config.ToConfig);

            bool sourceExistsInTo = await to.ExistsAsync(sourceKey);

            if (sourceExistsInTo) {
                return await to.MoveFileAsync(sourceKey, destKey, handleExisting);
            }
            
            var fromFile = await from.GetFileAsync(sourceKey);

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

        public async Task<IDeleteResult> DeleteFileAsync(string key, MigratorProviderConfig config) {
            var from = cabinetFactory.GetCabinet(config.FromConfig);
            var to = cabinetFactory.GetCabinet(config.ToConfig);

            var fromResult = await from.DeleteFileAsync(key);
            var toResult = await to.DeleteFileAsync(key);

            //TODO: Combine results

            return toResult;
        }

        private static async Task<Stream> CheckExistsThenOpenAsync(IFileCabinet cabinet, string key) {
            bool exists = await cabinet.ExistsAsync(key);
            if (exists) {
                return await cabinet.OpenReadStreamAsync(key);
            }

            return null;
        }
    }
}

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
    public class MigratorCabinet : IFileCabinet {
        private readonly IFileCabinet from;
        private readonly IFileCabinet to;

        public MigratorCabinet(IFileCabinet from, IFileCabinet to) {
            this.from = from;
            this.to = to;
        }

        public bool SupportsProviderType(string providerType) {
            return to.SupportsProviderType(providerType) || from.SupportsProviderType(providerType);
        }

        public async Task<bool> ExistsAsync(string key) {
            bool exists = await to.ExistsAsync(key);

            if (!exists) {
                exists = await from.ExistsAsync(key); // might not be migrated yet
            }

            return exists;
        }

        public async Task<IEnumerable<string>> ListKeysAsync(string keyPrefix = "", bool recursive = true) {
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

        public async Task<ICabinetItemInfo> GetFileAsync(string key) {
            var item = await to.GetFileAsync(key);

            if (!item.Exists) {
                item = await from.GetFileAsync(key);
            }

            return item;
        }

        public Task<IEnumerable<ICabinetItemInfo>> GetFilesAsync(string keyPrefix = "", bool recursive = true) {
            throw new NotImplementedException();
        }

        public async Task<Stream> OpenReadStreamAsync(ICabinetItemInfo file) {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var stream = await CheckExistsThenOpenAsync(to, file);
            if (stream == null) {
                stream = await CheckExistsThenOpenAsync(from, file);
            }

            return stream;
        }

        public async Task<ISaveResult> SaveFileAsync(string key, string filePath, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress = null) {
            return await to.SaveFileAsync(key, filePath, handleExisting, progress);
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress = null) {
            return await to.SaveFileAsync(key, content, handleExisting, progress);
        }

        public async Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting) {
            bool sourceExistsInTo = await to.ExistsAsync(sourceKey);

            if (sourceExistsInTo) {
                return await to.MoveFileAsync(sourceKey, destKey, handleExisting);
            }
            
            var fromFile = await from.GetFileAsync(sourceKey);

            if (!fromFile.Exists) {
                return new MoveResult(sourceKey, destKey, false, errorMsg: "Source file does not exist");
            }

            using (var stream = await from.OpenReadStreamAsync(fromFile)) {
                var saveResult = await to.SaveFileAsync(destKey, stream, handleExisting);

                return new MoveResult(
                    sourceKey, destKey,
                    success: saveResult.Success,
                    errorMsg: saveResult.GetErrorMessage()
                );
            }
        }

        public async Task<IDeleteResult> DeleteFileAsync(string key) {
            var fromResult = await from.DeleteFileAsync(key);
            var toResult = await to.DeleteFileAsync(key);

            //TODO: Combine results

            return toResult;
        }

        private static async Task<Stream> CheckExistsThenOpenAsync(IFileCabinet cabinet, ICabinetItemInfo file) {
            if (cabinet.SupportsProviderType(file.ProviderType)) {
                bool exists = await cabinet.ExistsAsync(file.Key);
                if (exists) {
                    return await cabinet.OpenReadStreamAsync(file);
                }
            }

            return null;
        }
    }
}

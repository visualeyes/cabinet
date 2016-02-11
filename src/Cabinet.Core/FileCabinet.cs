using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cabinet.Core.Providers;
using Cabinet.Core.Results;

namespace Cabinet.Core {
    internal class FileCabinet<T> : IFileCabinet where T : class, IStorageProviderConfig {
        private readonly T config;
        private readonly IStorageProvider<T> provider;

        public FileCabinet(IStorageProvider<T> provider, T config) {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (config == null) throw new ArgumentNullException(nameof(config));

            this.provider = provider;
            this.config = config;
        }

        public async Task<bool> ExistsAsync(string key) {
            return await provider.ExistsAsync(key, config);
        }

        public async Task<ICabinetItemInfo> GetItemAsync(string key) {
            return await provider.GetFileAsync(key, config);
        }

        public async Task<IEnumerable<ICabinetItemInfo>> GetItemAsync(string keyPrefix = "", bool recursive = true) {
            return await provider.GetItemsAsync(keyPrefix: keyPrefix, recursive: recursive, config: config);
        }

        public async Task<Stream> OpenReadStreamAsync(string key) {
            return await provider.OpenReadStreamAsync(key, config);
        }

        public async Task<IEnumerable<string>> ListKeysAsync(string keyPrefix = "", bool recursive = true) {
            return await provider.ListKeysAsync(keyPrefix: keyPrefix, recursive: recursive, config: config);
        }

        public async Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting) {
            return await provider.MoveFileAsync(sourceKey, destKey, handleExisting, config);
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress = null) {
            return await provider.SaveFileAsync(key, content, handleExisting, progress, config);
        }

        public async Task<ISaveResult> SaveFileAsync(string key, string filePath, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress = null) {
            return await provider.SaveFileAsync(key, filePath, handleExisting, progress, config);
        }

        public async Task<IDeleteResult> DeleteFileAsync(string key) {
            return await provider.DeleteFileAsync(key, config);
        }
    }
}

using Cabinet.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cabinet.Core;
using Cabinet.Core.Results;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using Cabinet.Azure.Results;

namespace Cabinet.Azure {
    public class AzureStorageProvider : IStorageProvider<AzureCabinetConfig> {
        private readonly IAzureClientFactory clientFactory;

        public string ProviderType {
            get { return AzureCabinetConfig.ProviderType; }
        }

        internal AzureStorageProvider(IAzureClientFactory clientFactory) {
            this.clientFactory = clientFactory;
        }

        public Task<bool> ExistsAsync(string key, AzureCabinetConfig config) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> ListKeysAsync(AzureCabinetConfig config, string keyPrefix = "", bool recursive = true) {
            throw new NotImplementedException();
        }

        public async Task<ICabinetItemInfo> GetFileAsync(string key, AzureCabinetConfig config) {
            var client = clientFactory.GetBlobClient(config);
            var container = client.GetContainerReference(config.Container);
            var blob = container.GetBlobReference(key);

            bool exists = await blob.ExistsAsync();
            
            return new AzureCabinetItemInfo(key, exists, ItemType.File, null);
        }

        public Task<IEnumerable<ICabinetItemInfo>> GetItemsAsync(AzureCabinetConfig config, string keyPrefix = "", bool recursive = true) {
            throw new NotImplementedException();
        }

        public async Task<Stream> OpenReadStreamAsync(string key, AzureCabinetConfig config) {
            var blob = GetBlob(key, config);

            var stream = await blob.OpenReadAsync();
            return stream;
        }

        public Task<ISaveResult> SaveFileAsync(string key, string filePath, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress, AzureCabinetConfig config) {
            throw new NotImplementedException();
        }

        public Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress, AzureCabinetConfig config) {
            throw new NotImplementedException();
        }

        public Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting, AzureCabinetConfig config) {
            throw new NotImplementedException();
        }

        public async Task<IDeleteResult> DeleteFileAsync(string key, AzureCabinetConfig config) {
            var blob = GetBlob(key, config);
            try {
                await blob.DeleteIfExistsAsync();
                return new DeleteResult();
            } catch(Exception e) {
                return new DeleteResult(e);
            }
        }

        private CloudBlob GetBlob(string key, AzureCabinetConfig config) {
            var client = clientFactory.GetBlobClient(config);
            var container = client.GetContainerReference(config.Container);
            var blob = container.GetBlobReference(key);
            return blob;
        }
    }
}

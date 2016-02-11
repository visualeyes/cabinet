using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cabinet.Core.Providers {
    public interface IReadonlyStorageProvider<T> where T : IStorageProviderConfig {
        string ProviderType { get; }

        Task<bool> ExistsAsync(string key, T config);
        Task<IEnumerable<string>> ListKeysAsync(T config, string keyPrefix = "", bool recursive = true);

        Task<ICabinetItemInfo> GetFileAsync(string key, T config);
        Task<IEnumerable<ICabinetItemInfo>> GetItemsAsync(T config, string keyPrefix = "", bool recursive = true);

        Task<Stream> OpenReadStreamAsync(string key, T config);
    }
}
using Cabinet.Core.Providers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cabinet.Core {
    public interface IReadonlyFileCabinet {
        string GetKeyDelimiter();

        Task<bool> ExistsAsync(string key);
        Task<IEnumerable<string>> ListKeysAsync(string keyPrefix = "", bool recursive = true);

        Task<ICabinetItemInfo> GetItemAsync(string key);
        Task<IEnumerable<ICabinetItemInfo>> GetItemsAsync(string keyPrefix = "", bool recursive = true);

        Task<Stream> OpenReadStreamAsync(string key);

    }
}
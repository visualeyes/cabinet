using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cabinet.Core {
    public interface IReadonlyFileCabinet {
        Task<bool> ExistsAsync(string key);
        Task<IEnumerable<string>> ListKeysAsync(string keyPrefix = "", bool recursive = true);

        Task<ICabinetItemInfo> GetItemAsync(string key);
        Task<IEnumerable<ICabinetItemInfo>> GetItemAsync(string keyPrefix = "", bool recursive = true);

        Task<Stream> OpenReadStreamAsync(string key);

    }
}
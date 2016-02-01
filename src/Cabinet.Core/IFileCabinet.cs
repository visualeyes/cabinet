using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Core {
    public interface IFileCabinet {
        bool SupportsProviderType(string providerType);

        Task<bool> ExistsAsync(string key);
        Task<IEnumerable<string>> ListKeysAsync(string keyPrefix = "", bool recursive = true);

        Task<ICabinetItemInfo> GetFileAsync(string key);
        Task<IEnumerable<ICabinetItemInfo>> GetFilesAsync(string keyPrefix = "", bool recursive = true);

        Task<Stream> OpenReadStreamAsync(ICabinetItemInfo file);

        Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress = null);
        Task<ISaveResult> SaveFileAsync(string key, string filePath, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress = null);

        Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting);
        Task<IDeleteResult> DeleteFileAsync(string key);
    }
}

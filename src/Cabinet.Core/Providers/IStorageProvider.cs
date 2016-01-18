using Cabinet.Core.Providers.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Core.Providers {
    public interface IStorageProvider {
        // Task<string> GetUniqueFileNameAsync(string path, string name);
        
        Task<bool> ExistsAsync(string key);
        Task<IEnumerable<string>> ListKeysAsync(string keyPrefix = "", bool recursive = true);

        Task<ICabinetFileInfo> GetFileAsync(string key);
        Task<IEnumerable<ICabinetFileInfo>> GetFilesAsync(string keyPrefix = "", bool recursive = true);

        Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting);
        Task<IMoveResult> MoveFileAsync(ICabinetFileInfo file, string destKey, HandleExistingMethod handleExisting);
        Task<IDeleteResult> DeleteFileAsync(string key);
    }
}

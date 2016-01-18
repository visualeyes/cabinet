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
        Task<ICabinetFileInfo> GetFileAsync(string key);

        Task<ISaveResult> SaveFileAsync(string key, Stream content, bool overwriteExisting);
        Task<IMoveResult> MoveFileAsync(ICabinetFileInfo file, string destKey, bool overwriteExisting);
        Task<IDeleteResult> DeleteFileAsync(string key);
    }
}

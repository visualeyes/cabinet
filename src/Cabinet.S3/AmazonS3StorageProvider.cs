using Cabinet.Core.Providers;
using Cabinet.Core.Providers.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cabinet.Core;
using System.IO;
using System.Net.Mime;

namespace Cabinet.S3 {
    public class AmazonS3StorageProvider : IStorageProvider {

        public Task<bool> ExistsAsync(string key) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> ListKeysAsync(string keyPrefix = null, bool recursive = true) {
            throw new NotImplementedException();
        }

        public Task<ICabinetFileInfo> GetFileAsync(string key) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ICabinetFileInfo>> GetFilesAsync(string keyPrefix = null, bool recursive = true) {
            throw new NotImplementedException();
        }

        public Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting) {
            throw new NotImplementedException();
        }

        public Task<IMoveResult> MoveFileAsync(ICabinetFileInfo file, string destKey, HandleExistingMethod handleExisting) {
            throw new NotImplementedException();
        }

        public Task<IDeleteResult> DeleteFileAsync(string key) {
            throw new NotImplementedException();
        }
    }
}

using Cabinet.Core.Results;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Cabinet.Core.Providers {
    /// <summary>
    /// Provides IO operations for a storage provider i.e. FileSystem, S3, AzureBlobStorage
    /// Ideally it should be implemented in a thread safe maner so it can be used as a singleton
    /// </summary>
    /// <typeparam name="T">Provider Configuration</typeparam>
    public interface IStorageProvider<T> : IReadonlyStorageProvider<T> where T : IStorageProviderConfig {

        Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress, T config);
        Task<ISaveResult> SaveFileAsync(string key, string filePath, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress, T config);

        Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting, T config);
        Task<IDeleteResult> DeleteFileAsync(string key, T config);
    }
}

using Cabinet.Core;
using Cabinet.Core.Providers;
using Cabinet.Core.Providers.Results;
using Cabinet.FileSystem.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.FileSystem {
    public class FileSystemStorageProvider : IStorageProvider {
        public const string ProviderType = "FileSystem";

        private readonly IFileCabinentConfig config;
        private readonly IFileSystem fs;

        public FileSystemStorageProvider(IFileCabinentConfig config, IFileSystem fileSystem) {
            this.config = config;
            this.fs = fileSystem;
        }
        
        /*
        public string GetUniqueFileName(string keyPrefix, string fileName) {
            // TODO - Prevent back paths
            string fullKeyPath = GetKeyFullPath(keyPrefix);
            
            string filePath = Path.Combine(fullKeyPath, fileName);

            string extensionlessName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            int count = 1;

            while (this.fs.File.Exists(filePath)) {
                filePath = Path.Combine(fullKeyPath, String.Format("{0} ({1}){2}", extensionlessName, count, extension));
                count++;
            }

            return filePath;
        }
        */

        public Task<bool> ExistsAsync(string key) {
            var fileInfo = this.GetFileInfo(key);
            return Task.FromResult(fileInfo.Exists);
        }

        public Task<IEnumerable<string>> ListKeysAsync(string keyPrefix = "", bool recursive = true) {
            var cabinetFiles = GetFilesRecursive(keyPrefix, recursive);
            var keys = cabinetFiles.Select(f => CabinetFileInfo.GetFileKey(f, config.Directory));

            return Task.FromResult(keys);
        }

        public Task<ICabinetFileInfo> GetFileAsync(string key) {
            var fileInfo = this.GetFileInfo(key);
            return Task.FromResult<ICabinetFileInfo>(new CabinetFileInfo(fileInfo, config.Directory));
        }

        public Task<IEnumerable<ICabinetFileInfo>> GetFilesAsync(string keyPrefix = "", bool recursive = true) {
            var cabinetFiles = GetFilesRecursive(keyPrefix, recursive);

            var cabinetFileInfos = cabinetFiles.Select(f => {
                return new CabinetFileInfo(f, config.Directory);
            });

            return Task.FromResult<IEnumerable<ICabinetFileInfo>>(cabinetFileInfos);
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting) {
            var fileInfo = this.GetFileInfo(key);

            var handleExisingResult = HandleExistingFile<ISaveResult>(fileInfo, key, handleExisting,
                () => new SaveResult(true) { AlreadyExists = true },
                (deleteResult) => new SaveResult(deleteResult.Exception, deleteResult.GetErrorMessage())
            );

            if(handleExisingResult != null) {
                return handleExisingResult;
            }

            try {
                using (var writeStream = this.fs.File.OpenWrite(fileInfo.FullName)) {
                    await content.CopyToAsync(writeStream);

                    return new SaveResult(true);
                }
            } catch (Exception e) {
                return new SaveResult(e);
            }
        }

        public async Task<IMoveResult> MoveFileAsync(ICabinetFileInfo file, string destKey, HandleExistingMethod handleExisting) {
            var destFileInfo = this.GetFileInfo(destKey);
            
            var handleExisingResult = HandleExistingFile<IMoveResult>(destFileInfo, destKey, handleExisting,
                () => new MoveResult(true) { AlreadyExists = true },
                (deleteResult) => new MoveResult(deleteResult.Exception, deleteResult.GetErrorMessage())
            );

            if (handleExisingResult != null) {
                return handleExisingResult;
            }

            try {

                if (file.ProviderType == ProviderType) {
                    var fileInfo = this.GetFileInfo(file.Key);
                    // Do file system move
                    this.fs.File.Move(fileInfo.FullName, destFileInfo.FullName);
                } else {
                    // Save file via stream
                    using (var fileStream = file.GetFileReadStream()) {
                        using (var destStream = this.fs.File.OpenWrite(destFileInfo.FullName)) {
                            await fileStream.CopyToAsync(destStream);
                        }
                    }
                }
                
                return new MoveResult(true);
            } catch (Exception e) {
                return new MoveResult(e);
            }
        }

        public Task<IDeleteResult> DeleteFileAsync(string key) {
            var fileInfo = this.GetFileInfo(key);

            if (!fileInfo.Exists) {
                return Task.FromResult<IDeleteResult>(new DeleteResult(true) {
                    AlreadyDeleted = true
                });
            }

            var result = DeleteFile(fileInfo);
            return Task.FromResult(result);
        }

        public FileInfoBase GetFileInfo(string key) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (String.IsNullOrWhiteSpace(config.Directory)) {
                throw new ApplicationException("The config directory has not been configured.");
            }

            string fullKeyPath = GetKeyFullPath(key);

            var keyFile = this.fs.FileInfo.FromFileName(fullKeyPath);
            var baseDir = this.fs.DirectoryInfo.FromDirectoryName(config.Directory);

            // Prevent backpaths
            if (!keyFile.Directory.IsSameDirectory(baseDir) && !keyFile.Directory.IsChildOf(baseDir)) {
                throw new ArgumentException(String.Format("{0} results in a path outside of {1}", key, baseDir.FullName), nameof(key));
            }

            return keyFile;
        }

        public DirectoryInfoBase GetDirectoryInfo(string key) {
            if (key == null) throw new ArgumentNullException(nameof(key)); // Allow empty strings
            if (String.IsNullOrWhiteSpace(config.Directory)) {
                throw new ApplicationException("The config directory has not been configured.");
            }

            string fullKeyPath = GetKeyFullPath(key);

            var keyFile = this.fs.DirectoryInfo.FromDirectoryName(fullKeyPath);
            var baseDir = this.fs.DirectoryInfo.FromDirectoryName(config.Directory);

            // Prevent backpaths
            if (!keyFile.IsSameDirectory(baseDir) && !keyFile.IsChildOf(baseDir)) {
                throw new ArgumentException(String.Format("{0} results in a path outside of {1}", key, baseDir.FullName), nameof(key));
            }

            return keyFile;
        }

        private string GetKeyFullPath(string key) {
            string keyPath = Path.Combine(config.Directory, key);
            string fullKeyPath = Path.GetFullPath(keyPath);

            return fullKeyPath;
        }

        private static IDeleteResult DeleteFile(FileInfoBase fileInfo) {
            try {
                fileInfo.Delete();
                return new DeleteResult(true);
            } catch (Exception e) {
                return new DeleteResult(e);
            }
        }
        
        private FileInfoBase[] GetFilesRecursive(string keyPrefix, bool recursive) {
            var dirInfo = this.GetDirectoryInfo(keyPrefix);

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = dirInfo.GetFiles("*", searchOption);
            return files;
        }

        private T HandleExistingFile<T>(FileInfoBase fileInfo, string key, HandleExistingMethod handleExisting, Func<T> createExisingResult, Func<IDeleteResult, T> createFailedDeleteResult) where T : class {
            if (!fileInfo.Exists) {
                return null;
            }

            if (handleExisting == HandleExistingMethod.Throw) {
                throw new ApplicationException(String.Format("File exists at {0} and handleExisting is set to throw", key));
            }

            if (handleExisting == HandleExistingMethod.Skip) {
                return createExisingResult();
            }

            if (handleExisting == HandleExistingMethod.Overwrite) {
                var deleteResult = DeleteFile(fileInfo);
                if (!deleteResult.Success) {
                    return createFailedDeleteResult(deleteResult);
                }

                return null;
            }

            throw new NotImplementedException();
        }
    }
}

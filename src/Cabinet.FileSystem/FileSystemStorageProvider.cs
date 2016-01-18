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

        public Task<bool> ExistsAsync(string key) {
            var fileInfo = this.GetFileInfo(key);
            return Task.FromResult(fileInfo.Exists);
        }

        public Task<ICabinetFileInfo> GetFileAsync(string key) {
            var fileInfo = this.GetFileInfo(key);
            return Task.FromResult<ICabinetFileInfo>(new CabinetFileInfo(key, fileInfo));
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, bool overwriteExisting) {
            var fileInfo = this.GetFileInfo(key);

            var deleteResult = DeleteExistingOrThrow(fileInfo, key, overwriteExisting);
            if (!deleteResult.Success) {
                return new SaveResult(deleteResult.Exception, deleteResult.GetErrorMessage());
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

        public async Task<IMoveResult> MoveFileAsync(ICabinetFileInfo file, string destKey, bool overwriteExisting) {
            var destFileInfo = this.GetFileInfo(destKey);

            var deleteResult = DeleteExistingOrThrow(destFileInfo, destKey, overwriteExisting);
            if (!deleteResult.Success) {
                return new MoveResult(deleteResult.Exception, deleteResult.GetErrorMessage());
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
            
            string keyPath = Path.Combine(config.Directory, key);
            string fullKeyPath = Path.GetFullPath(keyPath);

            var keyFile = this.fs.FileInfo.FromFileName(fullKeyPath);
            var baseDir = this.fs.DirectoryInfo.FromDirectoryName(config.Directory);

            // Prevent backpaths
            if (!keyFile.Directory.IsSameDirectory(baseDir) && !keyFile.Directory.IsChildOf(baseDir)) {
                throw new ArgumentException(String.Format("{0} results in a path outside of {1}", key, baseDir.FullName), nameof(key));
            }

            return keyFile;
        }

        private static IDeleteResult DeleteExistingOrThrow(FileInfoBase fileInfo, string key, bool overwriteExisting) {
            if (!fileInfo.Exists) {
                return new DeleteResult(true) {
                    AlreadyDeleted = true
                };
            }

            if (!overwriteExisting) {
                throw new ApplicationException(String.Format("File exists at {0} and overwriteExisting is false", key));
            }

            return DeleteFile(fileInfo);
        }

        private static IDeleteResult DeleteFile(FileInfoBase fileInfo) {
            try {
                fileInfo.Delete();
                return new DeleteResult(true);
            } catch (Exception e) {
                return new DeleteResult(e);
            }
        }
    }
}

using Cabinet.Core;
using Cabinet.Core.Providers;
using Cabinet.Core.Results;
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
    internal class FileSystemStorageProvider : IStorageProvider<FileSystemCabinetConfig> {
        public const string ProviderType = "FileSystem";
        
        private readonly Func<IFileSystem> fileSystemFactory;

        public FileSystemStorageProvider(Func<IFileSystem> fileSystemFactory) {
            this.fileSystemFactory = fileSystemFactory;
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

        public Task<bool> ExistsAsync(string key, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var fileInfo = this.GetFileInfo(key, config);
            return Task.FromResult(fileInfo.Exists);
        }

        public Task<IEnumerable<string>> ListKeysAsync(FileSystemCabinetConfig config, string keyPrefix = "", bool recursive = true) {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var cabinetFiles = GetFilesRecursive(keyPrefix, recursive, config);
            var keys = cabinetFiles.Select(f => FileSystemCabinetFileInfo.GetFileKey(f, config.Directory));

            return Task.FromResult(keys);
        }

        public Task<ICabinetFileInfo> GetFileAsync(string key, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var fileInfo = this.GetFileInfo(key, config);
            return Task.FromResult<ICabinetFileInfo>(new FileSystemCabinetFileInfo(fileInfo, config.Directory));
        }

        public Task<IEnumerable<ICabinetFileInfo>> GetFilesAsync(FileSystemCabinetConfig config, string keyPrefix = "", bool recursive = true) {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var cabinetFiles = GetFilesRecursive(keyPrefix, recursive, config);

            var cabinetFileInfos = cabinetFiles.Select(f => {
                return new FileSystemCabinetFileInfo(f, config.Directory);
            });

            return Task.FromResult<IEnumerable<ICabinetFileInfo>>(cabinetFileInfos);
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var fileInfo = this.GetFileInfo(key, config);

            var handleExisingResult = HandleExistingFile<ISaveResult>(fileInfo, key, handleExisting,
                () => new SaveResult(key, success: true) { AlreadyExists = true },
                (deleteResult) => new SaveResult(key, deleteResult.Exception, deleteResult.GetErrorMessage())
            );

            if(handleExisingResult != null) {
                return handleExisingResult;
            }

            try {
                var fs = GetFileSystem(config);
                
                if(!fileInfo.Directory.Exists) {
                    fileInfo.Directory.Create();
                }

                // If for some reason, a file with the same key is created,
                // only allow it to overwrite if overwriting is allowed
                var openMode = handleExisting == HandleExistingMethod.Overwrite ? FileMode.Create : FileMode.CreateNew;

                using (var writeStream = fs.File.Open(fileInfo.FullName, openMode, FileAccess.Write)) {
                    // no using block as this is simply a wrapper
                    
                    var progressWriteStream = new ProgressStream(writeStream, content.Length, progress);

                    await content.CopyToAsync(progressWriteStream);

                    return new SaveResult(key, success: true);
                }
            } catch (Exception e) {
                return new SaveResult(key, e);
            }
        }

        public async Task<ISaveResult> SaveFileAsync(string key, string filePath, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (String.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (config == null) throw new ArgumentNullException(nameof(config));

            try {
                var fs = GetFileSystem(config);
                var fileInfo = fs.FileInfo.FromFileName(filePath);

                if(!fileInfo.Exists) {
                    return new SaveResult(key, success: false, errorMsg: String.Format("File does not exist at path {0}", filePath));
                }

                using(var readStream = fileInfo.OpenRead()) {
                    return await this.SaveFileAsync(key, readStream, handleExisting, progress, config);
                }
            } catch (Exception e) {
                return new SaveResult(key, e);
            }
        }

        public Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(sourceKey)) throw new ArgumentNullException(nameof(sourceKey));
            if (String.IsNullOrWhiteSpace(destKey)) throw new ArgumentNullException(nameof(destKey));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var destFileInfo = this.GetFileInfo(destKey, config);
            
            var handleExisingResult = HandleExistingFile<IMoveResult>(destFileInfo, destKey, handleExisting,
                () => new MoveResult(sourceKey, destKey, success: true) { AlreadyExists = true },
                (deleteResult) => new MoveResult(sourceKey, destKey, deleteResult.Exception, deleteResult.GetErrorMessage())
            );

            if (handleExisingResult != null) {
                return Task.FromResult(handleExisingResult);
            }

            try {
                var fs = GetFileSystem(config);

                var fileInfo = this.GetFileInfo(sourceKey, config);

                if (!destFileInfo.Directory.Exists) {
                    destFileInfo.Directory.Create();
                }

                // Do file system move
                fs.File.Move(fileInfo.FullName, destFileInfo.FullName);
                
                return Task.FromResult<IMoveResult>(new MoveResult(sourceKey, destKey, success: true));
            } catch (Exception e) {
                return Task.FromResult<IMoveResult>(new MoveResult(sourceKey, destKey, e));
            }
        }

        public Task<IDeleteResult> DeleteFileAsync(string key, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var fileInfo = this.GetFileInfo(key, config);

            if (!fileInfo.Exists) {
                return Task.FromResult<IDeleteResult>(new DeleteResult(true) {
                    AlreadyDeleted = true
                });
            }

            var result = DeleteFile(fileInfo);
            return Task.FromResult(result);
        }

        public FileInfoBase GetFileInfo(string key, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (String.IsNullOrWhiteSpace(config.Directory)) {
                throw new ApplicationException("The config directory has not been configured.");
            }

            string fullKeyPath = GetKeyFullPath(config.Directory, key);

            var fs = GetFileSystem(config);

            var keyFile = fs.FileInfo.FromFileName(fullKeyPath);
            var baseDir = fs.DirectoryInfo.FromDirectoryName(config.Directory);

            // Prevent backpaths
            if (!keyFile.Directory.IsSameDirectory(baseDir) && !keyFile.Directory.IsChildOf(baseDir)) {
                throw new ArgumentException(String.Format("{0} results in a path outside of {1}", key, baseDir.FullName), nameof(key));
            }

            return keyFile;
        }

        public DirectoryInfoBase GetDirectoryInfo(string key, FileSystemCabinetConfig config) {
            if (key == null) throw new ArgumentNullException(nameof(key)); // Allow empty strings
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (String.IsNullOrWhiteSpace(config.Directory)) {
                throw new ApplicationException("The config directory has not been configured.");
            }

            string fullKeyPath = GetKeyFullPath(config.Directory, key);
            var fs = GetFileSystem(config);

            var keyFile = fs.DirectoryInfo.FromDirectoryName(fullKeyPath);
            var baseDir = fs.DirectoryInfo.FromDirectoryName(config.Directory);

            // Prevent backpaths
            if (!keyFile.IsSameDirectory(baseDir) && !keyFile.IsChildOf(baseDir)) {
                throw new ArgumentException(String.Format("{0} results in a path outside of {1}", key, baseDir.FullName), nameof(key));
            }

            return keyFile;
        }

        private IFileSystem GetFileSystem(FileSystemCabinetConfig config) {
            var fs = fileSystemFactory();

            bool directoryExists = fs.Directory.Exists(config.Directory);

            if (!directoryExists) {
                if(!config.CreateIfNotExists) {
                    throw new ApplicationException(String.Format("{0} does not exist and CreateIfNotExists is set to false.", config.Directory));
                }

                fs.Directory.CreateDirectory(config.Directory);
            }

            return fs;
        }

        private string GetKeyFullPath(string directory, string key) {
            if (String.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException(nameof(directory));
            if (key == null) throw new ArgumentNullException(nameof(key)); // Allow empty strings

            string keyPath = Path.Combine(directory, key);
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
        
        private FileInfoBase[] GetFilesRecursive(string keyPrefix, bool recursive, FileSystemCabinetConfig config) {
            var dirInfo = this.GetDirectoryInfo(keyPrefix, config);
                        
            if(!dirInfo.Exists) {
                return new FileInfoBase[0];
            }
            
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

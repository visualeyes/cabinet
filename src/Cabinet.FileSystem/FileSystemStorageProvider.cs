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
        
        private readonly Func<IFileSystem> fileSystemFactory;

        string IStorageProvider<FileSystemCabinetConfig>.ProviderType {
            get { return FileSystemCabinetConfig.ProviderType; }
        }

        public FileSystemStorageProvider(Func<IFileSystem> fileSystemFactory) {
            this.fileSystemFactory = fileSystemFactory;
        }

        public Task<bool> ExistsAsync(string key, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var fileInfo = this.GetFileInfo(key, config);
            return Task.FromResult(fileInfo.Exists);
        }

        public Task<IEnumerable<string>> ListKeysAsync(FileSystemCabinetConfig config, string keyPrefix = "", bool recursive = true) {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var cabinetFiles = GetItemsRecursive(keyPrefix, recursive, config);
            var keys = cabinetFiles
                        .Where(f => f is FileInfoBase) // we only want file keys
                        .Select(f => FileSystemCabinetItemInfo.GetItemKey(f, config.Directory));

            return Task.FromResult(keys);
        }

        public Task<ICabinetItemInfo> GetFileAsync(string key, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var fileInfo = this.GetFileInfo(key, config);
            return Task.FromResult<ICabinetItemInfo>(new FileSystemCabinetItemInfo(fileInfo, config.Directory));
        }

        public Task<IEnumerable<ICabinetItemInfo>> GetItemsAsync(FileSystemCabinetConfig config, string keyPrefix = "", bool recursive = true) {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var cabinetFiles = GetItemsRecursive(keyPrefix, recursive, config);

            var cabinetFileInfos = cabinetFiles.Select(f => {
                return new FileSystemCabinetItemInfo(f, config.Directory);
            });

            return Task.FromResult<IEnumerable<ICabinetItemInfo>>(cabinetFileInfos);
        }

        public Task<Stream> OpenReadStreamAsync(string key, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));


            var fs = GetFileSystem(config);

            var fileInfo = this.GetFileInfo(key, config);
            var stream = fileInfo.OpenRead();
            return Task.FromResult(stream);
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (config == null) throw new ArgumentNullException(nameof(config));

            // If for some reason, a file with the same key is created,
            // only allow it to overwrite if overwriting is allowed
            var openMode = handleExisting == HandleExistingMethod.Overwrite ? FileMode.Create : FileMode.CreateNew;

            try {
                var fileInfo = this.GetFileInfo(key, config);

                var fs = GetFileSystem(config);

                if (!fileInfo.Directory.Exists) {
                    fileInfo.Directory.Create();
                }

                using (var writeStream = fs.File.Open(fileInfo.FullName, openMode, FileAccess.Write)) {
                    // no using block as this is simply a wrapper
                    var progressWriteStream = new ProgressStream(writeStream, content.Length, progress);

                    await content.CopyToAsync(progressWriteStream);

                    return new SaveResult(key, success: true);
                }
            } catch(DirectoryNotFoundException e) {
                return new SaveResult(key, e);
            } catch(IOException e) {
                // We tried to create a new file but it already exists
                if(openMode == FileMode.CreateNew) {
                    if (handleExisting == HandleExistingMethod.Throw) {
                        throw new ApplicationException(String.Format("File exists at {0} and handleExisting is set to throw", key));
                    }

                    if (handleExisting == HandleExistingMethod.Skip) {
                        return new SaveResult(key, success: true) { AlreadyExists = true };
                    }
                }
                
                // Failed for some other reason so return an error
                return new SaveResult(key, e);
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

                using (var readStream = fs.File.OpenRead(filePath)) {
                    return await this.SaveFileAsync(key, readStream, handleExisting, progress, config);
                }
            } catch (FileNotFoundException e) {
                return new SaveResult(key, success: false, errorMsg: String.Format("File does not exist at path {0}", filePath));
            } catch (Exception e) {
                return new SaveResult(key, e);
            }
        }

        public async Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(sourceKey)) throw new ArgumentNullException(nameof(sourceKey));
            if (String.IsNullOrWhiteSpace(destKey)) throw new ArgumentNullException(nameof(destKey));
            if (config == null) throw new ArgumentNullException(nameof(config));

            if (handleExisting == HandleExistingMethod.Overwrite) {
                return await MoveAndOverwrite(sourceKey, destKey, config);
            }
            
            return await MoveInternal(sourceKey, destKey, handleExisting, config);
        }

        public Task<IDeleteResult> DeleteFileAsync(string key, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));

            try {
                string fullKeyPath = GetKeyFullPath(config, key);

                var fs = GetFileSystem(config);

                fs.File.Delete(fullKeyPath);

                return Task.FromResult<IDeleteResult>(new DeleteResult());
            } catch (DirectoryNotFoundException) {
                return Task.FromResult<IDeleteResult>(new DeleteResult());
            } catch (Exception e) {
                return Task.FromResult<IDeleteResult>(new DeleteResult(e));
            }
        }

        public FileInfoBase GetFileInfo(string key, FileSystemCabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));

            string fullKeyPath = GetKeyFullPath(config, key);

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

            string fullKeyPath = GetKeyFullPath(config, key);
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

        private string GetKeyFullPath(FileSystemCabinetConfig config, string key) {
            Contract.NotNull(config, nameof(config));
            if (key == null) throw new ArgumentNullException(nameof(key)); // Allow empty strings

            string keyPath = Path.Combine(config.Directory, key);
            string fullKeyPath = Path.GetFullPath(keyPath);

            return fullKeyPath;
        }
                
        private IEnumerable<FileSystemInfoBase> GetItemsRecursive(string keyPrefix, bool recursive, FileSystemCabinetConfig config) {
            if (keyPrefix == null) keyPrefix = "";

            var dirInfo = this.GetDirectoryInfo(keyPrefix, config);

            if (!dirInfo.Exists) {
                return Enumerable.Empty<FileSystemInfoBase>();
            }
            
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var itemInfoEnumeration =  dirInfo.EnumerateFileSystemInfos("*", searchOption);

            if(recursive) {
                itemInfoEnumeration = itemInfoEnumeration.Where(i => i is FileInfoBase);
            }

            return itemInfoEnumeration;
        }

        private async Task<IMoveResult> MoveAndOverwrite(string sourceKey, string destKey, FileSystemCabinetConfig config) {
            try {
                ISaveResult saveResult = null;

                // Close stream as soon as we have a result
                using (var stream = await this.OpenReadStreamAsync(sourceKey, config)) {
                    saveResult = await this.SaveFileAsync(destKey, stream, HandleExistingMethod.Overwrite, null, config);
                }

                if (!saveResult.Success) {
                    return new MoveResult(sourceKey, destKey, success: false, errorMsg: saveResult.GetErrorMessage());
                }
                
                var deleteResult = await this.DeleteFileAsync(sourceKey, config);

                if (!deleteResult.Success) {
                    return new MoveResult(sourceKey, destKey, success: false, errorMsg: deleteResult.GetErrorMessage());
                }

                return new MoveResult(sourceKey, destKey);
            } catch (Exception e) {
                return new MoveResult(sourceKey, destKey, e);
            }
        }

        private Task<IMoveResult> MoveInternal(string sourceKey, string destKey, HandleExistingMethod handleExisting, FileSystemCabinetConfig config) {
            var destFileInfo = this.GetFileInfo(destKey, config);

            try {
                var fs = GetFileSystem(config);

                var fileInfo = this.GetFileInfo(sourceKey, config);

                if (!destFileInfo.Directory.Exists) {
                    destFileInfo.Directory.Create();
                }

                // Do file system move
                fs.File.Move(fileInfo.FullName, destFileInfo.FullName);

                return Task.FromResult<IMoveResult>(new MoveResult(sourceKey, destKey, success: true));
            } catch(DirectoryNotFoundException e) {
                return Task.FromResult<IMoveResult>(new MoveResult(sourceKey, destKey, e));
            } catch (IOException e) {
                if (handleExisting == HandleExistingMethod.Throw) {
                    throw new ApplicationException(String.Format("File exists at {0} and handleExisting is set to throw", destKey));
                }

                if (handleExisting == HandleExistingMethod.Skip) {
                    return Task.FromResult<IMoveResult>(new MoveResult(sourceKey, destKey, success: true) {
                        AlreadyExists = true
                    });
                }

                return Task.FromResult<IMoveResult>(new MoveResult(sourceKey, destKey, e));
            } catch (Exception e) {
                return Task.FromResult<IMoveResult>(new MoveResult(sourceKey, destKey, e));
            }
        }
    }
}

using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cabinet.Core;
using System.IO;
using System.Net.Mime;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net;
using Cabinet.S3.Results;
using System.Threading;
using Amazon.S3.Transfer;
using Cabinet.Core.Exceptions;

namespace Cabinet.S3 {
    internal class AmazonS3StorageProvider : IStorageProvider<AmazonS3CabinetConfig> {
        private readonly IAmazonS3ClientFactory clientFactory;

        public string ProviderType => AmazonS3CabinetConfig.ProviderType;

        internal AmazonS3StorageProvider(IAmazonS3ClientFactory clientFactory) {
            this.clientFactory = clientFactory;
        }

        public async Task<bool> ExistsAsync(string key, AmazonS3CabinetConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));

            using (var s3Client = GetS3Client(config)) {
                return await ExistsInternalAsync(key, config, s3Client);
            }
        }

        public async Task<IEnumerable<string>> ListKeysAsync(AmazonS3CabinetConfig config, string keyPrefix = null, bool recursive = true) {
            Contract.NotNull(config, nameof(config));

            using (var s3Client = GetS3Client(config)) {
                var s3Objects = await GetS3Objects(keyPrefix, recursive, config, s3Client);

                return s3Objects.Select(o => o.Key);
            }
        }

        public async Task<ICabinetItemInfo> GetFileAsync(string key, AmazonS3CabinetConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));

            using (var s3Client = GetS3Client(config)) {
                using (var response = await GetS3Object(key, config, s3Client, CancellationToken.None)) {
                    bool exists = response.HttpStatusCode == HttpStatusCode.OK;
                    
                    return new AmazonS3CabinetItemInfo(key, exists, ItemType.File) {
                        Size = response.ContentLength,
                        LastModifiedUtc = response.LastModified.ToUniversalTime()
                    };
                }
            }
        }

        public async Task<IEnumerable<ICabinetItemInfo>> GetItemsAsync(AmazonS3CabinetConfig config, string keyPrefix = null, bool recursive = true) {
            Contract.NotNull(config, nameof(config));

            using (var s3Client = GetS3Client(config)) {
                var s3Objects = await GetS3Objects(keyPrefix, recursive, config, s3Client);

                return s3Objects;
            }
        }

        public async Task<Stream> OpenReadStreamAsync(string key, AmazonS3CabinetConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));

            using (var s3Client = GetS3Client(config)) {
                var transferUtility = GetTransferUtility(s3Client);
                string itemKey = GetKey(key, config);

                try {
                    return await transferUtility.OpenStreamAsync(config.BucketName, itemKey);
                } catch(Exception e) {
                    throw new CabinetFileOpenException(itemKey, e);
                }
            }
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<IWriteProgress> progress, AmazonS3CabinetConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(content, nameof(content));
            Contract.NotNull(config, nameof(config));

            if(handleExisting != HandleExistingMethod.Overwrite) {
                throw new NotImplementedException();
            }

            using (var s3Client = GetS3Client(config)) {
                try {
                    // Use the transfer utility as it handle large files in a better way
                    var uploadRequest = new TransferUtilityUploadRequest {
                        InputStream = content,
                    };
                    
                    await UploadInternal(key, config, s3Client, progress, uploadRequest);

                    return new SaveResult(key);
                } catch (Exception e) {
                    return new SaveResult(key, e);
                }
            }
        }

        public async Task<ISaveResult> SaveFileAsync(string key, string filePath, HandleExistingMethod handleExisting, IProgress<IWriteProgress> progress, AmazonS3CabinetConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNullOrEmpty(filePath, nameof(filePath));
            Contract.NotNull(config, nameof(config));

            if (handleExisting != HandleExistingMethod.Overwrite) {
                throw new NotImplementedException();
            }

            using (var s3Client = GetS3Client(config)) {
                try {
                    // Use the transfer utility as it handle large files in a better way
                    var uploadRequest = new TransferUtilityUploadRequest {
                        FilePath = filePath,
                    };
                    
                    await UploadInternal(key, config, s3Client, progress, uploadRequest);

                    return new SaveResult(key);
                } catch (Exception e) {
                    return new SaveResult(key, e);
                }
            }
        }

        public async Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting, AmazonS3CabinetConfig config) {
            Contract.NotNullOrEmpty(sourceKey, nameof(sourceKey));
            Contract.NotNullOrEmpty(destKey, nameof(destKey));
            Contract.NotNull(config, nameof(config));

            if (handleExisting != HandleExistingMethod.Overwrite) {
                throw new NotImplementedException();
            }

            using (var s3Client = GetS3Client(config)) {
                try {
                    var copyRequest = new CopyObjectRequest {
                        SourceBucket = config.BucketName,
                        DestinationBucket = config.BucketName,
                        SourceKey = GetKey(sourceKey, config),
                        DestinationKey = GetKey(destKey, config)
                    };

                    var response = await s3Client.CopyObjectAsync(copyRequest);

                    if (response.HttpStatusCode == HttpStatusCode.OK) {
                        var deleteResult = await DeleteInternal(sourceKey, config, s3Client);

                        if (!deleteResult.Success) {
                            return new MoveResult(sourceKey, destKey, deleteResult.Exception, deleteResult.GetErrorMessage());
                        }
                    }

                    return new MoveResult(sourceKey, destKey, response.HttpStatusCode);
                } catch (Exception e) {
                    return new MoveResult(sourceKey, destKey, e);
                }
            }
        }

        public async Task<IDeleteResult> DeleteFileAsync(string key, AmazonS3CabinetConfig config) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));

            using (var s3Client = GetS3Client(config)) {
                return await DeleteInternal(key, config, s3Client);
            }
        }

        private static async Task<bool> ExistsInternalAsync(string key, AmazonS3CabinetConfig config, IAmazonS3 s3Client) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));
            Contract.NotNull(s3Client, nameof(s3Client));

            using (var response = await GetS3Object(key, config, s3Client, CancellationToken.None)) {
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
        }
        
        private async Task UploadInternal(string key, AmazonS3CabinetConfig config, IAmazonS3 s3Client, IProgress<IWriteProgress> progress, TransferUtilityUploadRequest uploadRequest) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));
            Contract.NotNull(s3Client, nameof(s3Client));

            var utilty = GetTransferUtility(s3Client);

            uploadRequest.BucketName = config.BucketName;
            uploadRequest.Key = GetKey(key, config);

            uploadRequest.UploadProgressEvent += (sender, e) => {
                progress?.Report(new WriteProgress(key, e.TransferredBytes, e.TotalBytes));
            };
            
            await utilty.UploadAsync(uploadRequest);
        }

        private static async Task<IEnumerable<AmazonS3CabinetItemInfo>> GetS3Objects(string keyPrefix, bool recursive, AmazonS3CabinetConfig config, IAmazonS3 s3Client) {
            Contract.NotNull(config, nameof(config));
            Contract.NotNull(s3Client, nameof(s3Client));

            var fileInfos = new List<AmazonS3CabinetItemInfo>();
            
            // If a delimiter is not specified, the response will include all keys i.e. recursive
            // If a delimiter is specified, the response will return files under the prefix and the CommonPrefixes

            var request = new ListObjectsRequest {
                BucketName = config.BucketName,
                Prefix = GetKey(keyPrefix, config)
            };

            if (!recursive) {
                request.Delimiter = config.Delimiter;
            }

            do {
                var response = await s3Client.ListObjectsAsync(request);

                var directories = response.CommonPrefixes.Select(prefix => new AmazonS3CabinetItemInfo(prefix, true, ItemType.Directory));

                var files = response.S3Objects.Select(o => new AmazonS3CabinetItemInfo(o.Key, true, ItemType.File) {
                    Size = o.Size,
                    LastModifiedUtc = o.LastModified.ToUniversalTime()
                });
                
                fileInfos.AddRange(directories);
                fileInfos.AddRange(files);

                if (response.IsTruncated) {
                    request.Marker = response.NextMarker;
                } else {
                    request = null;
                }
            } while (request != null);

            return fileInfos;
        }

        private static async Task<GetObjectResponse> GetS3Object(string key, AmazonS3CabinetConfig config, IAmazonS3 s3Client, CancellationToken cancellationToken) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));
            Contract.NotNull(s3Client, nameof(s3Client));

            var request = new GetObjectRequest {
                BucketName = config.BucketName,
                Key = GetKey(key, config)
            };

            using (var response = await s3Client.GetObjectAsync(request, cancellationToken)) {
                return response;
            }
        }

        private static async Task<IDeleteResult> DeleteInternal(string key, AmazonS3CabinetConfig config, IAmazonS3 s3Client) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(config, nameof(config));
            Contract.NotNull(s3Client, nameof(s3Client));

            try {
                var deleteObjectRequest = new DeleteObjectRequest {
                    BucketName = config.BucketName,
                    Key = GetKey(key, config)
                };

                var response = await s3Client.DeleteObjectAsync(deleteObjectRequest);

                return new DeleteResult(response.HttpStatusCode);
            } catch (Exception e) {
                return new DeleteResult(e);
            }
        }

        private IAmazonS3 GetS3Client(AmazonS3CabinetConfig config) {
            Contract.NotNull(config, nameof(config));

            return clientFactory.GetS3Client(config);
        }

        private ITransferUtility GetTransferUtility(IAmazonS3 client) {
            Contract.NotNull(client, nameof(client));

            return clientFactory.GetTransferUtility(client);
        }

        private static string GetKey(string key, AmazonS3CabinetConfig config) {
            return KeyUtils.JoinKeys(config.KeyPrefix, key, config.Delimiter);
        }
    }
}

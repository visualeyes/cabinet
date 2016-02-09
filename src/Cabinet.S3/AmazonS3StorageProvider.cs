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

namespace Cabinet.S3 {
    internal class AmazonS3StorageProvider : IStorageProvider<AmazonS3CabinetConfig> {
        private readonly IAmazonS3ClientFactory clientFactory;

        public string ProviderType {
            get { return AmazonS3CabinetConfig.ProviderType; }
        }

        internal AmazonS3StorageProvider(IAmazonS3ClientFactory clientFactory) {
            this.clientFactory = clientFactory;
        }

        public async Task<bool> ExistsAsync(string key, AmazonS3CabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var s3Client = GetS3Client(config)) {
                return await ExistsInternalAsync(key, config, s3Client);
            }
        }

        public async Task<IEnumerable<string>> ListKeysAsync(AmazonS3CabinetConfig config, string keyPrefix = null, bool recursive = true) {
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var s3Client = GetS3Client(config)) {
                var s3Objects = await GetS3Objects(keyPrefix, recursive, config, s3Client);

                return s3Objects.Select(o => o.Key);
            }
        }

        public async Task<ICabinetItemInfo> GetFileAsync(string key, AmazonS3CabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var s3Client = GetS3Client(config)) {
                using (var response = await GetS3Object(key, config, s3Client, CancellationToken.None)) {
                    bool exists = response.HttpStatusCode == HttpStatusCode.OK;
                    
                    return new AmazonS3CabinetItemInfo(response.Key, exists, ItemType.File, response.LastModified.ToUniversalTime());
                }
            }
        }

        public async Task<IEnumerable<ICabinetItemInfo>> GetItemsAsync(AmazonS3CabinetConfig config, string keyPrefix = null, bool recursive = true) {
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var s3Client = GetS3Client(config)) {
                var s3Objects = await GetS3Objects(keyPrefix, recursive, config, s3Client);

                return s3Objects;
            }
        }

        public async Task<Stream> OpenReadStreamAsync(string key, AmazonS3CabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var s3Client = GetS3Client(config)) {
                var transferUtility = GetTransferUtility(s3Client);
                return await transferUtility.OpenStreamAsync(config.BucketName, key);
            }
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress, AmazonS3CabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var s3Client = GetS3Client(config)) {
                bool skip = await SkipUploadAsync(key, handleExisting, config, s3Client);

                if (skip) {
                    return new SaveResult(key) { AlreadyExists = true };
                }

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

        public async Task<ISaveResult> SaveFileAsync(string key, string filePath, HandleExistingMethod handleExisting, IProgress<WriteProgress> progress, AmazonS3CabinetConfig config) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (String.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var s3Client = GetS3Client(config)) {
                bool skip = await SkipUploadAsync(key, handleExisting, config, s3Client);

                if (skip) {
                    return new SaveResult(key) { AlreadyExists = true };
                }

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
            if (String.IsNullOrWhiteSpace(sourceKey)) throw new ArgumentNullException(nameof(sourceKey));
            if (String.IsNullOrWhiteSpace(destKey)) throw new ArgumentNullException(nameof(destKey));
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var s3Client = GetS3Client(config)) {
                bool skip = await SkipUploadAsync(destKey, handleExisting, config, s3Client);

                if (skip) {
                    return new MoveResult(sourceKey, destKey) { AlreadyExists = true };
                }

                try {
                    var copyRequest = new CopyObjectRequest {
                        SourceBucket = config.BucketName,
                        DestinationBucket = config.BucketName,
                        SourceKey = sourceKey,
                        DestinationKey = destKey
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
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var s3Client = GetS3Client(config)) {
                return await DeleteInternal(key, config, s3Client);
            }
        }

        private static async Task<bool> ExistsInternalAsync(string key, AmazonS3CabinetConfig config, IAmazonS3 s3Client) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s3Client == null) throw new ArgumentNullException(nameof(s3Client));

            using (var response = await GetS3Object(key, config, s3Client, CancellationToken.None)) {
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
        }

        private static async Task<bool> SkipUploadAsync(string key, HandleExistingMethod handleExisting, AmazonS3CabinetConfig config, IAmazonS3 s3Client) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s3Client == null) throw new ArgumentNullException(nameof(s3Client));

            if (handleExisting == HandleExistingMethod.Overwrite) {
                return false;
            }

            bool exists = await ExistsInternalAsync(key, config, s3Client);
            if (!exists) return false; // nothing to do

            if (handleExisting == HandleExistingMethod.Skip) {
                return true;
            } else if (handleExisting == HandleExistingMethod.Throw) {
                throw new ApplicationException(String.Format("File exists at {0} and handleExisting is set to throw", key));
            }

            throw new NotImplementedException();
        }

        private async Task UploadInternal(string key, AmazonS3CabinetConfig config, IAmazonS3 s3Client, IProgress<WriteProgress> progress, TransferUtilityUploadRequest uploadRequest) {
            var utilty = GetTransferUtility(s3Client);

            uploadRequest.BucketName = config.BucketName;
            uploadRequest.Key = key;

            uploadRequest.UploadProgressEvent += (object sender, UploadProgressArgs e) => {
                // TODO: more helpful progress
                // e.PercentDone;
                // e.TotalBytes;
                progress?.Report(new WriteProgress {
                    BytesWritten = e.TransferredBytes
                });
            };

            await utilty.UploadAsync(uploadRequest);
        }

        private static async Task<IEnumerable<AmazonS3CabinetItemInfo>> GetS3Objects(string keyPrefix, bool recursive, AmazonS3CabinetConfig config, IAmazonS3 s3Client) {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s3Client == null) throw new ArgumentNullException(nameof(s3Client));

            var fileInfos = new List<AmazonS3CabinetItemInfo>();
            
            // If a delimiter is not specified, the response will include all keys i.e. recursive
            // If a delimiter is specified, the response will return files under the prefix and the CommonPrefixes

            var request = new ListObjectsRequest {
                BucketName = config.BucketName,
                Prefix = keyPrefix
            };

            if (!recursive) {
                request.Delimiter = config.Delimiter;
            }

            do {
                var response = await s3Client.ListObjectsAsync(request);

                var directories = response.CommonPrefixes.Select(prefix => new AmazonS3CabinetItemInfo(prefix, true, ItemType.Directory, null));
                var files = response.S3Objects.Select(o => new AmazonS3CabinetItemInfo(o.Key, true, ItemType.File, o.LastModified.ToUniversalTime()));

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
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s3Client == null) throw new ArgumentNullException(nameof(s3Client));

            var request = new GetObjectRequest {
                BucketName = config.BucketName,
                Key = key
            };

            using (var response = await s3Client.GetObjectAsync(request, cancellationToken)) {
                return response;
            }
        }

        private static async Task<IDeleteResult> DeleteInternal(string key, AmazonS3CabinetConfig config, IAmazonS3 s3Client) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s3Client == null) throw new ArgumentNullException(nameof(s3Client));

            try {
                var deleteObjectRequest = new DeleteObjectRequest {
                    BucketName = config.BucketName,
                    Key = key
                };

                var response = await s3Client.DeleteObjectAsync(deleteObjectRequest);

                return new DeleteResult(response.HttpStatusCode);
            } catch (Exception e) {
                return new DeleteResult(e);
            }
        }

        private IAmazonS3 GetS3Client(AmazonS3CabinetConfig config) {
            if (config == null) throw new ArgumentNullException(nameof(config));

            return clientFactory.GetS3Client(config);
        }

        private ITransferUtility GetTransferUtility(IAmazonS3 client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            return clientFactory.GetTransferUtility(client);
        }
    }
}

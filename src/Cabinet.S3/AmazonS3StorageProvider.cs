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

namespace Cabinet.S3 {
    // TOCONSIDER: Should GetFile ect get an object or just return info that can be used to get the stream
    internal class AmazonS3StorageProvider : IStorageProvider<S3CabinetConfig> {
        public const string ProviderType = "AmazonS3";

        private readonly IS3ClientFactory clientFactory;

        internal AmazonS3StorageProvider(IS3ClientFactory clientFactory) {
            this.clientFactory = clientFactory;
        }

        public async Task<bool> ExistsAsync(string key, S3CabinetConfig config) {
            using (var s3Client = GetS3Client(config)) {
                return await ExistsInternalAsync(key, config, s3Client);
            }
        }

        public async Task<IEnumerable<string>> ListKeysAsync(S3CabinetConfig config, string keyPrefix = null, bool recursive = true) {
            using (var s3Client = GetS3Client(config)) {
                var s3Objects = await GetS3Objects(config, keyPrefix, s3Client);
                
                return s3Objects.Select(o => o.Key);
            }
        }

        public async Task<ICabinetFileInfo> GetFileAsync(string key, S3CabinetConfig config) {
            using (var s3Client = GetS3Client(config)) {
                using (var response = await GetS3Object(s3Client, key, config, CancellationToken.None)) {
                    return new S3CabinetFileInfo(response);
                }
            }
        }

        public Task<IEnumerable<ICabinetFileInfo>> GetFilesAsync(S3CabinetConfig config, string keyPrefix = null, bool recursive = true) {
            throw new NotImplementedException();
        }

        public async Task<ISaveResult> SaveFileAsync(string key, Stream content, HandleExistingMethod handleExisting, S3CabinetConfig config) {
            using (var s3Client = GetS3Client(config)) {
                bool skip = await SkipUploadAsync(key, handleExisting, config, s3Client);

                if (skip) {
                    return new SaveResult() {
                        AlreadyExists = true
                    };
                }

                try {
                    var response = await UploadInternal(key, content, config, s3Client);

                    return new SaveResult(response.HttpStatusCode);
                } catch (Exception e) {
                    return new SaveResult(e);
                }
            }
        }

        public async Task<IMoveResult> MoveFileAsync(string sourceKey, string destKey, HandleExistingMethod handleExisting, S3CabinetConfig config) {
            if (String.IsNullOrWhiteSpace(sourceKey)) throw new ArgumentNullException(nameof(sourceKey));
            if (String.IsNullOrWhiteSpace(destKey)) throw new ArgumentNullException(nameof(destKey));

            using (var s3Client = GetS3Client(config)) {
                bool skip = await SkipUploadAsync(destKey, handleExisting, config, s3Client);

                if (skip) {
                    return new MoveResult() { AlreadyExists = true };
                }

                try {
                    var copyRequest = new CopyObjectRequest {
                        SourceBucket = config.BucketName,
                        DestinationBucket = config.BucketName,
                        SourceKey = sourceKey,
                        DestinationKey = destKey
                    };

                    var response = await s3Client.CopyObjectAsync(copyRequest);

                    if(response.HttpStatusCode == HttpStatusCode.OK) {
                        var deleteResult = await DeleteInternal(sourceKey, config, s3Client);

                        if (!deleteResult.Success) {
                            return new MoveResult(deleteResult.Exception, deleteResult.GetErrorMessage());
                        }
                    }

                    return new MoveResult(response.HttpStatusCode);
                } catch (Exception e) {
                    return new MoveResult(e);
                }
            }
        }

        public async Task<IDeleteResult> DeleteFileAsync(string key, S3CabinetConfig config) {
            using (var s3Client = GetS3Client(config)) {
                return await DeleteInternal(key, config, s3Client);
            }
        }

        private static async Task<bool> ExistsInternalAsync(string key, S3CabinetConfig config, IAmazonS3 s3Client) {
            using (var response = await GetS3Object(s3Client, key, config, CancellationToken.None)) {
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
        }

        private static async Task<bool> SkipUploadAsync(string key, HandleExistingMethod handleExisting, S3CabinetConfig config, IAmazonS3 s3Client) {
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

        private static async Task<List<S3Object>> GetS3Objects(S3CabinetConfig config, string keyPrefix, IAmazonS3 s3Client) {
            var s3Objects = new List<S3Object>();

            var request = new ListObjectsRequest {
                BucketName = config.BucketName,
                Prefix = keyPrefix,
            };

            do {
                var response = await s3Client.ListObjectsAsync(request);

                s3Objects.AddRange(s3Objects);

                if (response.IsTruncated) {
                    request.Marker = response.NextMarker;
                } else {
                    request = null;
                }
            } while (request != null);

            return s3Objects;
        }

        private static async Task<GetObjectResponse> GetS3Object(IAmazonS3 s3Client, string key, S3CabinetConfig config, CancellationToken cancellationToken) {
            var request = new GetObjectRequest {
                BucketName = config.BucketName,
                Key = key
            };

            using (var response = await s3Client.GetObjectAsync(request, cancellationToken)) {
                return response;
            }
        }

        private static async Task<PutObjectResponse> UploadInternal(string key, Stream content, S3CabinetConfig config, IAmazonS3 s3Client) {
            var request = new PutObjectRequest {
                BucketName = config.BucketName,
                Key = key,
                InputStream = content
            };

            var response = await s3Client.PutObjectAsync(request);
            return response;
        }

        private static async Task<IDeleteResult> DeleteInternal(string key, S3CabinetConfig config, IAmazonS3 s3Client) {
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

        private IAmazonS3 GetS3Client(S3CabinetConfig config) {
            return clientFactory.GetS3Client(config);
        }
    }
}

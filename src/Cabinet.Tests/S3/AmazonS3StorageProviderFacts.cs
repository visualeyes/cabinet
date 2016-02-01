using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Cabinet.Core;
using Cabinet.Core.Providers;
using Cabinet.S3;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.S3 {
    public class AmazonS3StorageProviderFacts {
        private readonly Mock<IAmazonS3ClientFactory> mockS3ClientFactory;
        private readonly Mock<IAmazonS3> mockS3Client;
        private const string ValidBucketName = "bucket-name";
        private const string ValidFileKey = "key";

        public AmazonS3StorageProviderFacts() {
            this.mockS3ClientFactory = new Mock<IAmazonS3ClientFactory>();
            this.mockS3Client = new Mock<IAmazonS3>();

            this.mockS3ClientFactory.Setup(f => f.GetS3Client(It.IsAny<AmazonS3CabinetConfig>())).Returns(this.mockS3Client.Object);
        }

        [Fact]
        public void Provider_Type() {
            IStorageProvider<AmazonS3CabinetConfig> provider = GetProvider();
            Assert.Equal(AmazonS3StorageProvider.ProviderType, provider.ProviderType);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Exists_Empty_Key_Throws(string key) {
            var provider = GetProvider();
            var config = GetConfig(ValidBucketName);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.ExistsAsync(key, config));
        }

        [Fact]
        public async Task Exists_Null_Config_Throws() {
            var provider = GetProvider();
            AmazonS3CabinetConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.ExistsAsync(ValidFileKey, config));
        }

        [Theory]
        [InlineData("test-bucket", "test-key", HttpStatusCode.OK, true)]
        [InlineData("test-bucket", "test-key", HttpStatusCode.NotFound, false)]
        [InlineData("test-bucket", "test-key", HttpStatusCode.Forbidden, false)]
        [InlineData("test-bucket", "test-key", HttpStatusCode.Unauthorized, false)]
        public async Task Exists(string bucketName, string key, HttpStatusCode code, bool expectedExists) {
            var provider = GetProvider();
            var config = GetConfig(bucketName);

            SetupGetObjectRequest(bucketName, key, code);

            bool actualExists = await provider.ExistsAsync(key, config);
            Assert.Equal(expectedExists, expectedExists);
        }

        [Fact]
        public async Task List_Keys_Null_Config_Throws() {
            var provider = GetProvider();
            AmazonS3CabinetConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.ListKeysAsync(config));
        }

        [Theory]
        [MemberData("GetTestS3Objects")]
        public async Task List_Keys(string bucketName, string keyPrefix, bool recursive, HttpStatusCode code, List<S3Object> s3Objects, List<S3Object> expectedS3Objects) {
            var provider = GetProvider();
            var config = GetConfig(bucketName);

            SetupGetObjectsRequest(bucketName, keyPrefix, code, s3Objects);

            var keys = await provider.ListKeysAsync(config, keyPrefix: keyPrefix, recursive: recursive);
            var keysList = keys.ToList();
            var expectedKeys = expectedS3Objects.Select(o => o.Key).ToList();
            Assert.Equal(expectedKeys, keysList);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Get_File_Empty_Key_Throws(string key) {
            var provider = GetProvider();
            var config = GetConfig(ValidBucketName);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.GetFileAsync(key, config));
        }

        [Fact]
        public async Task Get_File_Null_Config_Throws() {
            var provider = GetProvider();
            AmazonS3CabinetConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.GetFileAsync(ValidFileKey, config));
        }

        [Theory]
        [InlineData("test-bucket", "test-key", HttpStatusCode.OK, true)]
        [InlineData("test-bucket", "test-key", HttpStatusCode.NotFound, false)]
        [InlineData("test-bucket", "test-key", HttpStatusCode.Forbidden, false)]
        [InlineData("test-bucket", "test-key", HttpStatusCode.Unauthorized, false)]
        public async Task Get_File(string bucketName, string key, HttpStatusCode code, bool expectedExists) {
            var provider = GetProvider();
            var config = GetConfig(bucketName);

            SetupGetObjectRequest(bucketName, key, code);

            var file = await provider.GetFileAsync(key, config);

            Assert.Equal(key, file.Key);
            Assert.Equal(expectedExists, file.Exists);
        }

        [Fact]
        public async Task Get_Items_Null_Config_Throws() {
            var provider = GetProvider();
            AmazonS3CabinetConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.GetItemsAsync(config));
        }

        [Theory]
        [MemberData("GetTestS3Objects")]
        public async Task Get_Items(string bucketName, string keyPrefix, bool recursive, HttpStatusCode code, List<S3Object> s3Objects, List<S3Object> expectedS3Objects) {
            var provider = GetProvider();
            var config = GetConfig(bucketName);

            SetupGetObjectsRequest(bucketName, keyPrefix, code, s3Objects);

            var expectedFileInfos = expectedS3Objects.Select(o => new AmazonS3CabinetItemInfo(o.Key, true, ItemType.File));

            var fileInfos = await provider.GetItemsAsync(config, keyPrefix: keyPrefix, recursive: recursive);
            
            Assert.Equal(expectedFileInfos, fileInfos, new S3CabinetFileInfoKeyComparer());
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Open_Read_Stream_Empty_Key_Throws(string key) {
            var provider = GetProvider();
            var config = GetConfig(ValidBucketName);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.OpenReadStreamAsync(key, config));
        }

        [Fact]
        public async Task Open_Read_Stream_Null_Config_Throws() {
            var provider = GetProvider();
            AmazonS3CabinetConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.OpenReadStreamAsync(ValidFileKey, config));
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Save_File_Path_Empty_Key_Throws(string key) {
            var provider = GetProvider();
            var config = GetConfig(ValidBucketName);
            string filePath = @"C:\test\test.txt";
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await provider.SaveFileAsync(key, filePath, HandleExistingMethod.Overwrite, mockProgress.Object, config)
            );
        }
        
        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Save_File_Path_Empty_FilePath_Throws(string filePath) {
            var provider = GetProvider();
            var config = GetConfig(ValidBucketName);
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await provider.SaveFileAsync(ValidFileKey, filePath, HandleExistingMethod.Overwrite, mockProgress.Object, config)
            );
        }

        [Fact]
        public async Task Save_File_Stream_Path_Config_Throws() {
            var provider = GetProvider();
            AmazonS3CabinetConfig config = null;
            string filePath = @"C:\test\test.txt";
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await provider.SaveFileAsync(ValidFileKey, filePath, HandleExistingMethod.Overwrite, mockProgress.Object, config)
            );
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Save_File_Stream_Empty_Key_Throws(string key) {
            var provider = GetProvider();
            var config = GetConfig(ValidBucketName);
            var mockStream = new Mock<Stream>();
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await provider.SaveFileAsync(key, mockStream.Object, HandleExistingMethod.Overwrite, mockProgress.Object, config)
            );
        }

        [Fact]
        public async Task Save_File_Stream_Null_Config_Throws() {
            var provider = GetProvider();
            AmazonS3CabinetConfig config = null;
            var mockStream = new Mock<Stream>();
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await provider.SaveFileAsync(ValidFileKey, mockStream.Object, HandleExistingMethod.Overwrite, mockProgress.Object, config)
            );
        }


        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Move_File_Path_Empty_SourceKey_Throws(string sourceKey) {
            var provider = GetProvider();
            var config = GetConfig(ValidBucketName);
            string destKey = @"test.txt";
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await provider.MoveFileAsync(sourceKey, destKey, HandleExistingMethod.Overwrite, config)
            );
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Move_File_Path_Empty_DestKey_Throws(string destKey) {
            var provider = GetProvider();
            var config = GetConfig(ValidBucketName);
            string sourceKey = @"test.txt";
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await provider.MoveFileAsync(sourceKey, destKey, HandleExistingMethod.Overwrite, config)
            );
        }

        [Fact]
        public async Task Move_File_Path_Empty_Key_Throws() {
            var provider = GetProvider();
            AmazonS3CabinetConfig config = null;
            string sourceKey = @"source.txt";
            string destKey = @"dest.txt";
            var mockProgress = new Mock<IProgress<WriteProgress>>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await provider.MoveFileAsync(sourceKey, destKey, HandleExistingMethod.Overwrite, config)
            );
        }


        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task Delete_File_Empty_Key_Throws(string key) {
            var provider = GetProvider();
            var config = GetConfig(ValidBucketName);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.DeleteFileAsync(key, config));
        }

        [Fact]
        public async Task Delete_File_Null_Config_Throws() {
            var provider = GetProvider();
            AmazonS3CabinetConfig config = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await provider.DeleteFileAsync(ValidFileKey, config));
        }

        private void SetupGetObjectRequest(string bucketName, string key, HttpStatusCode code) {
            this.mockS3Client.Setup(
                s3 => s3.GetObjectAsync(It.Is<GetObjectRequest>((r) => r.BucketName == bucketName && r.Key == key), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new GetObjectResponse() {
                HttpStatusCode = code,
                Key = key
            });
        }

        private void SetupGetObjectsRequest(string bucketName, string keyPrefix, HttpStatusCode code, List<S3Object> s3Objects) {
            this.mockS3Client.Setup(
                s3 => s3.ListObjectsAsync(It.Is<ListObjectsRequest>((r) => r.BucketName == bucketName && r.Prefix == keyPrefix), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new ListObjectsResponse() {
                HttpStatusCode = code,
                S3Objects = s3Objects
            });
        }

        private AmazonS3StorageProvider GetProvider() {
            var provider = new AmazonS3StorageProvider(this.mockS3ClientFactory.Object);
            return provider;
        }

        private AmazonS3CabinetConfig GetConfig(string bucketName) {
            var config = new AmazonS3CabinetConfig(bucketName, RegionEndpoint.APSoutheast2, null);
            return config;
        }

        public static object[] GetTestS3Objects() {
            var s3Objects = new List<S3Object>() {
                new S3Object() { Key = "file.txt", Size = 3, LastModified = DateTime.UtcNow.AddHours(-1) },
                new S3Object() { Key = @"bar/one.txt", Size = 3, LastModified = DateTime.UtcNow.AddHours(-5) },
                new S3Object() { Key = @"bar/two.txt", Size = 3, LastModified = DateTime.UtcNow.AddMinutes(-1) },
                new S3Object() { Key = @"bar/baz/three", Size = 3, LastModified = DateTime.UtcNow.AddHours(-8) },
                new S3Object() { Key = @"foo/one.txt", Size = 3, LastModified = DateTime.UtcNow.AddSeconds(-10) },
            };

            // ListObjectsRequest has no concept of recurisve or not, it just is recursive
            // The response under the prefix will be all the keys with that prefix

            string barPrefix = "bar";

            var barObjects = s3Objects.Where(o => o.Key.StartsWith(barPrefix)).ToList();
            //var barDirectChildObjects = barObjects.Where(o => o.Key);

            return new object[] {
                new object[] { "test-bucket", "", true, HttpStatusCode.OK, s3Objects, s3Objects },
                //new object[] { "test-bucket", barPrefix, true, HttpStatusCode.OK, barObjects,barObjects },
                //new object[] { "test-bucket", "", false, HttpStatusCode.OK, s3Objects, s3Objects.Select(o => o.Key == "file.txt").ToList() },
                //new object[] { "test-bucket", barPrefix, true, HttpStatusCode.OK, barObjects, barObjects },
            };
        }
    }
}

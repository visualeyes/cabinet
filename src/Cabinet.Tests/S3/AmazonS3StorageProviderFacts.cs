using Amazon.S3;
using Amazon.S3.Model;
using Cabinet.Core.Providers;
using Cabinet.S3;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.S3 {
    public class AmazonS3StorageProviderFacts {
        private readonly Mock<IS3ClientFactory> mockS3ClientFactory;
        private readonly Mock<IAmazonS3> mockS3Client;

        public AmazonS3StorageProviderFacts() {
            this.mockS3ClientFactory = new Mock<IS3ClientFactory>();
            this.mockS3Client = new Mock<IAmazonS3>();

            this.mockS3ClientFactory.Setup(f => f.GetS3Client(It.IsAny<S3CabinetConfig>())).Returns(this.mockS3Client.Object);
        }

        [Fact]
        public void Provider_Type() {
            IStorageProvider<S3CabinetConfig> provider = GetProvider();
            Assert.Equal(AmazonS3StorageProvider.ProviderType, provider.ProviderType);
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

        private void SetupGetObjectRequest(string bucketName, string key, HttpStatusCode code) {
            this.mockS3Client.Setup(
                s3 => s3.GetObjectAsync(It.Is<GetObjectRequest>((r) => r.BucketName == bucketName && r.Key == key), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new GetObjectResponse() {
                HttpStatusCode = code
            });
        }

        private AmazonS3StorageProvider GetProvider() {
            var provider = new AmazonS3StorageProvider(this.mockS3ClientFactory.Object);
            return provider;
        }

        private S3CabinetConfig GetConfig(string bucketName) {
            var config = new S3CabinetConfig() {
                BucketName = bucketName
            };

            return config;
        }
    }
}

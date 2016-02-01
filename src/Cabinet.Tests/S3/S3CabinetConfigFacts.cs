using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Cabinet.S3;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.S3 {
    public class S3CabinetConfigFacts {
        private const string ValidBucketName = "bucket-name";

        private readonly Mock<AmazonS3Config> mockS3Config;
        private readonly Mock<AWSCredentials> mockS3Credentials;

        public S3CabinetConfigFacts() {
            this.mockS3Config = new Mock<AmazonS3Config>();
            this.mockS3Credentials = new Mock<AWSCredentials>();
        }

        [Fact]
        public void Get_Set_S3_Config() {
            var config = new AmazonS3CabinetConfig(ValidBucketName, mockS3Config.Object, mockS3Credentials.Object);

            Assert.Equal(mockS3Config.Object, config.AmazonS3Config);
        }

        [Fact]
        public void Get_Set_Bucket_Name() {
            var config = new AmazonS3CabinetConfig(ValidBucketName, mockS3Config.Object, mockS3Credentials.Object);

            Assert.Equal(ValidBucketName, config.BucketName);
        }
    }
}

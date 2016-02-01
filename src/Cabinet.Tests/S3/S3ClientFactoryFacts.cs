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
    public class S3ClientFactoryFacts {
        private const string ValidBucketName = "bucket-name";

        [Fact]
        public void Sets_Credentails_And_Config() {
            var s3Config = new AmazonS3Config() {
                RegionEndpoint = Amazon.RegionEndpoint.USEast1,
            };
            
            var mockCredentials = new Mock<AWSCredentials>();
            var factory = new AmazonS3ClientFactory();

            var config = new AmazonS3CabinetConfig(ValidBucketName, s3Config, mockCredentials.Object);

            var client = factory.GetS3Client(config) as AmazonS3Client;

            Assert.NotNull(client);
            Assert.Equal(s3Config, client.Config);
        }
    }
}

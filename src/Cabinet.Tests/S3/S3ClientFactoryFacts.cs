using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
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

        private readonly AmazonS3ClientFactory factory;

        public S3ClientFactoryFacts() {
            this.factory = new AmazonS3ClientFactory();
        }

        [Fact]
        public void Get_S3_Client_Sets_Credentails_And_Config() {
            var s3Config = new AmazonS3Config() {
                RegionEndpoint = Amazon.RegionEndpoint.USEast1,
            };
            
            var mockCredentials = new Mock<AWSCredentials>();

            var config = new AmazonS3CabinetConfig(ValidBucketName, s3Config, mockCredentials.Object);

            var client = factory.GetS3Client(config) as AmazonS3Client;

            Assert.NotNull(client);
            Assert.Equal(s3Config, client.Config);
        }


        [Fact]
        public void Get_Transfer_Utility() {
            var mockClient = new Mock<IAmazonS3>();
            
            var transferUtility = factory.GetTransferUtility(mockClient.Object) as TransferUtility;
            
            Assert.NotNull(transferUtility);
            Assert.Equal(mockClient.Object, transferUtility.S3Client);
        }
    }
}

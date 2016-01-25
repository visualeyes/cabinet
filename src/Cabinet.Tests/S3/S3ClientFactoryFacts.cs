using Amazon.S3;
using Cabinet.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
namespace Cabinet.Tests.S3 {
    public class S3ClientFactoryFacts {

        [Fact]
        public void Sets_Config() {
            var s3Config = new AmazonS3Config() {
                RegionEndpoint = Amazon.RegionEndpoint.USEast1
            };
            var factory = new S3ClientFactory();

            var client = factory.GetS3Client(new S3CabinetConfig {
                AmazonS3Config = s3Config
            }) as AmazonS3Client;

            Assert.NotNull(client);
            Assert.Equal(s3Config, client.Config);
        }
    }
}

using Amazon.S3;
using Cabinet.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.S3 {
    public class S3CabinetConfigFacts {

        [Fact]
        public void Get_Set_S3_Config() {
            var s3Config = new AmazonS3Config();

            var config = new S3CabinetConfig() {
                AmazonS3Config = s3Config
            };

            Assert.Equal(s3Config, config.AmazonS3Config);
        }

        [Fact]
        public void Get_Set_Bucket_Name() {
            string bucketName = "bucketName";

            var config = new S3CabinetConfig() {
                BucketName = bucketName
            };

            Assert.Equal(bucketName, config.BucketName);
        }
    }
}

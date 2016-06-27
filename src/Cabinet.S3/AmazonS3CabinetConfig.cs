using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Cabinet.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3 {
    public class AmazonS3CabinetConfig : IStorageProviderConfig {
        public const string ProviderType = "AmazonS3";

        public const string DefaultDelimiter = "/"; 

        public AmazonS3CabinetConfig(string bucketName, RegionEndpoint region, AWSCredentials credentials)
            :this(bucketName, new AmazonS3Config() { RegionEndpoint = region }, credentials) {
        }

        public AmazonS3CabinetConfig(string bucketName, AmazonS3Config s3Config, AWSCredentials credentials) {
            this.AWSCredentials = credentials;
            this.AmazonS3Config = s3Config;
            this.BucketName = bucketName;
            this.Delimiter = DefaultDelimiter;
        }

        public AWSCredentials AWSCredentials { get; }
        public AmazonS3Config AmazonS3Config { get; }

        public string BucketName { get; }

        public string KeyPrefix { get; set; }
        public string Delimiter { get; set; }
    }
}

using Amazon.S3;
using Cabinet.Core.Providers;

namespace Cabinet.S3 {
    public interface IS3CabinetConfig : IProviderConfiguration {
        AmazonS3Config AmazonS3Config { get; }
        string BucketName { get; }
    }
}
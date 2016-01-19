using Cabinet.Core.Providers;

namespace Cabinet.S3 {
    public interface IS3CabinetConfig : IProviderConfiguration {
        string AccessKey { get; }
        string SecretKey { get; }
    }
}
using Cabinet.Core.Providers;

namespace Cabinet.FileSystem {
    public interface IFileCabinentConfig : IProviderConfiguration {
        string Directory { get; set; }
    }
}
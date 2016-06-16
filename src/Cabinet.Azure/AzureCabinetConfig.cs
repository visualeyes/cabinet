using Cabinet.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cabinet.Azure {
    public class AzureCabinetConfig : IStorageProviderConfig {
        public const string ProviderType = "Azure";

        public AzureCabinetConfig(string connectionString, string container) {
            this.ConnectionString = connectionString;
            this.Container = container;
        }

        public string ConnectionString { get; private set; }
        public string Container { get; private set; }
    }
}

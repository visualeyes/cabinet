using Cabinet.Core;
using Cabinet.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator {
    public class MigratorProviderConfig : IStorageProviderConfig {
        public const string ProviderType = "Migrator";

        public IStorageProviderConfig FromConfig { get; set; }
        public IStorageProviderConfig ToConfig { get; set; }
    }
}

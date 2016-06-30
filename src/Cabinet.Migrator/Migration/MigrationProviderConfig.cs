using Cabinet.Core;
using Cabinet.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator.Migration {
    public class MigrationProviderConfig : IStorageProviderConfig {
        public const string ProviderType = "Migrator";

        public MigrationProviderConfig(IStorageProviderConfig from, IStorageProviderConfig to, string delimiter = null) {
            Contract.NotNull(from, nameof(from));
            Contract.NotNull(to, nameof(to));

            this.Delimiter = !String.IsNullOrWhiteSpace(delimiter) ? delimiter : from.Delimiter;

            this.From = from;
            this.To = to;
        }

        public string Delimiter { get; }

        public IStorageProviderConfig From { get; set; }
        public IStorageProviderConfig To { get; set; }
    }
}

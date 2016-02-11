using Cabinet.Core;
using Cabinet.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator.Replication {
    public class ReaplicatedProviderConfig : IStorageProviderConfig {
        public const string ProviderType = "Reaplicated";

        public ReaplicatedProviderConfig(IStorageProviderConfig master, IStorageProviderConfig replica) {
            Contract.NotNull(master, nameof(master));
            Contract.NotNull(replica, nameof(replica));

            this.Master = master;
            this.Replica = replica;
        }

        public IStorageProviderConfig Master { get; set; }
        public IStorageProviderConfig Replica { get; set; }
    }
}

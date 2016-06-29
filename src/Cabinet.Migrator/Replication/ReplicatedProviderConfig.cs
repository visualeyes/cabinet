using Cabinet.Core;
using Cabinet.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator.Replication {
    public class ReplicatedProviderConfig : IStorageProviderConfig {
        public const string ProviderType = "Replicated";

        public ReplicatedProviderConfig(IStorageProviderConfig master, IStorageProviderConfig replica) {
            Contract.NotNull(master, nameof(master));
            Contract.NotNull(replica, nameof(replica));

            this.Master = master;
            this.Replica = replica;
        }

        public IStorageProviderConfig Master { get; }
        public IStorageProviderConfig Replica { get; }
    }
}

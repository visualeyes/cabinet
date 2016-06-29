using Cabinet.Core;
using Cabinet.Migrator.Migration;
using Cabinet.Migrator.Replication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator {
    public static class FileCabinetFactoryExtensions {

        public static IFileCabinetFactory RegisterMigratorProvider(this IFileCabinetFactory factory) {
            var provider = new MigrationStorageProvider(factory);
            factory.RegisterProvider(provider);
            return factory;
        }

        public static IFileCabinetFactory RegisterReplicationProvider(this IFileCabinetFactory factory) {
            var provider = new ReplicatedStorageProvider(factory);
            factory.RegisterProvider(provider);
            return factory;
        }
    }
}

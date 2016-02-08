using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator {
    public static class FileCabinetFactoryExtensions {

        public static IFileCabinetFactory RegisterMigratorProvider(this IFileCabinetFactory factory) {
            var provider = new MigratorStorageProvider(factory);
            factory.RegisterProvider(provider);
            return factory;
        }
    }
}

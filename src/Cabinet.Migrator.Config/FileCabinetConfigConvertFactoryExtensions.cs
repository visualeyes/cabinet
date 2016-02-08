using Cabinet.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator.Config {
    public static class FileCabinetConfigConvertFactoryExtensions {

        public static IFileCabinetConfigConvertFactory RegisterMigratorConfigConverter(this IFileCabinetConfigConvertFactory factory, ICabinetProviderConfigStore store) {
            var migratorConverter = new MigratorProviderConfigConverter(store);
            
            factory.RegisterProvider(MigratorProviderConfig.ProviderType, migratorConverter);
            return factory;
        }
    }
}

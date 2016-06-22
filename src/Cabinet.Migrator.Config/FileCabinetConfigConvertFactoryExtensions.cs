using Cabinet.Config;
using Cabinet.Migrator.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator.Config {
    public static class FileCabinetConfigConvertFactoryExtensions {

        public static IFileCabinetConfigConverterFactory RegisterMigratorConfigConverter(this IFileCabinetConfigConverterFactory factory, ICabinetProviderConfigStore store) {
            var migratorConverter = new MigratorProviderConfigConverter(store);
            
            factory.RegisterProvider(MigrationProviderConfig.ProviderType, migratorConverter);
            return factory;
        }
    }
}

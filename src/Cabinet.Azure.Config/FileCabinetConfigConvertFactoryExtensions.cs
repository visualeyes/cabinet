using Cabinet.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Azure.Config {
    public static class FileCabinetConfigConvertFactoryExtensions {

        public static IFileCabinetConfigConvertFactory RegisterAzureConfigConverter(this IFileCabinetConfigConvertFactory factory) {
            factory.RegisterProvider(AzureCabinetConfig.ProviderType, new AzureConfigConverter());
            return factory;
        }
    }
}

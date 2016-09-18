using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Azure {
    public static class FileCabinetFactoryExtensions {

        public static IFileCabinetFactory RegisterAzureProvider(this IFileCabinetFactory factory) {
            var clientFactory = new AzureClientFactory();
            var provider = new AzureStorageProvider(clientFactory);
            factory.RegisterProvider(provider);
            return factory;
        }
    }
}

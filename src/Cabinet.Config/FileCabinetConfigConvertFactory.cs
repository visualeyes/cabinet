using Cabinet.Core.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Config {
    public class FileCabinetConfigConvertFactory : IFileCabinetConfigConvertFactory {
        private static ConcurrentDictionary<string, ICabinetProviderConfigConverter> converterCache = new ConcurrentDictionary<string, ICabinetProviderConfigConverter>();

        public ICabinetProviderConfigConverter GetConverter(string providerType) {
            if (String.IsNullOrWhiteSpace(providerType)) throw new ArgumentNullException(nameof(providerType));

            ICabinetProviderConfigConverter converter;

            if (!converterCache.TryGetValue(providerType, out converter)) {
                throw new ApplicationException("No converter is registered for a config of name: " + providerType);
            }
            
            return converter;
        }

        public void RegisterProvider(string providerType, ICabinetProviderConfigConverter converter) {
            if (String.IsNullOrWhiteSpace(providerType)) throw new ArgumentNullException(nameof(providerType));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            
            converterCache.AddOrUpdate(providerType, converter, (key, existing) => converter);
        }
    }
}

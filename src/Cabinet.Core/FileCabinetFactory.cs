using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cabinet.Core.Providers;
using System.Collections.Concurrent;

namespace Cabinet.Core {
    public class FileCabinetFactory : IFileCabinetFactory {
        private static ConcurrentDictionary<Type, Func<object>> providerCache = new ConcurrentDictionary<Type, Func<object>>();

        public IFileCabinet GetCabinet<T>(T config) where T : IProviderConfiguration {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var configType = typeof(T); // this is what it was registered with - config.GetType() would get the implementation

            Func<object> providerFactory;

            if (!providerCache.TryGetValue(configType, out providerFactory)) {
                throw new ApplicationException("No provider is registered for a config of type: " + configType.FullName);
            }

            var provider = providerFactory() as IStorageProvider<T>;

            if(provider == null) {
                throw new ApplicationException("An item in the cache could not be cast to it's correct type: " + configType.FullName);
            }
            
            var cabinet = new FileCabinet<T>(provider, config);
            return cabinet;
        }

        public void RegisterProvider<T>(Func<IStorageProvider<T>> providerFactory) where T : IProviderConfiguration {
            if (providerFactory == null) throw new ArgumentNullException(nameof(providerFactory));

            providerCache.AddOrUpdate(typeof(T), providerFactory, (key, existing) => providerFactory);
        }

        public void RegisterProvider<T>(IStorageProvider<T> provider) where T : IProviderConfiguration {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            Func<IStorageProvider<T>> providerFactory = () => provider;
            this.RegisterProvider(providerFactory);
        }
    }
}

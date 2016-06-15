using Cabinet.Core.Providers;
using System;
using System.Collections.Concurrent;

namespace Cabinet.Core {
    public class FileCabinetFactory : IFileCabinetFactory {
        private static ConcurrentDictionary<Type, Func<object>> providerCache = new ConcurrentDictionary<Type, Func<object>>();

        public IFileCabinet GetCabinet(IStorageProviderConfig config) {
            Contract.NotNull(config, nameof(config));

            Type configType = config.GetType();

            return CreateFileCabinet(config, configType);
        }

        public void RegisterProvider<T>(Func<IStorageProvider<T>> providerFactory) where T : class, IStorageProviderConfig {
            if (providerFactory == null) throw new ArgumentNullException(nameof(providerFactory));

            providerCache.AddOrUpdate(typeof(T), providerFactory, (key, existing) => providerFactory);
        }

        public void RegisterProvider<T>(IStorageProvider<T> provider) where T : class, IStorageProviderConfig {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            Func<IStorageProvider<T>> providerFactory = () => provider;
            this.RegisterProvider(providerFactory);
        }

        private static IFileCabinet CreateFileCabinet(IStorageProviderConfig config, Type configType) {
            Func<object> providerFactory;

            if (!providerCache.TryGetValue(configType, out providerFactory)) {
                throw new Exception("No provider is registered for a config of type: " + configType.FullName);
            }

            var provider = providerFactory();

            if(provider == null) {
                throw new Exception("The providerFactory returned null. Factory registered  for type: " + configType.FullName);
            }

            var cabinetType = typeof(FileCabinet<>);
            var cabinetGenericType = cabinetType.MakeGenericType(configType);

            var cabinet = Activator.CreateInstance(cabinetGenericType, provider, config);

            return cabinet as IFileCabinet;
        }

        internal void ClearCache() {
            providerCache.Clear();
        }
    }
}

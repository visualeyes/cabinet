using Cabinet.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Core {
    public interface IFileCabinetFactory {
        IFileCabinet GetCabinet(IStorageProviderConfig config);

        void RegisterProvider<T>(IStorageProvider<T> provider) where T : class, IStorageProviderConfig;
        void RegisterProvider<T>(Func<IStorageProvider<T>> providerFactory) where T : class, IStorageProviderConfig;
    }
}

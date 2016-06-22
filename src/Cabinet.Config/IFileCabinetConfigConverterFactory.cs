using System;
using Cabinet.Core.Providers;

namespace Cabinet.Config {
    public interface IFileCabinetConfigConverterFactory {
        ICabinetProviderConfigConverter GetConverter(string providerType);
        void RegisterProvider(string providerType, ICabinetProviderConfigConverter converter);
    }
}
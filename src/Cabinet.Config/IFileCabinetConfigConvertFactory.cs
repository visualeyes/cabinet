using System;
using Cabinet.Core.Providers;

namespace Cabinet.Config {
    public interface IFileCabinetConfigConvertFactory {
        ICabinetProviderConfigConverter GetConverter(string providerType);
        void RegisterProvider(string providerType, ICabinetProviderConfigConverter converter);
    }
}
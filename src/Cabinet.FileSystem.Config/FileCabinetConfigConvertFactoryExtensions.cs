using Cabinet.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.FileSystem.Config {
    public static class FileCabinetConfigConvertFactoryExtensions {

        public static IFileCabinetConfigConverterFactory RegisterFileSystemConfigConverter(this IFileCabinetConfigConverterFactory factory, IPathMapper pathMapper) {
            factory.RegisterProvider(FileSystemCabinetConfig.ProviderType, new FileSystemConfigConverter(pathMapper));
            return factory;
        }
    }
}

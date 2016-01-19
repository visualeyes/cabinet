using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.FileSystem {
    public static class FileCabinetFactoryExtensions {

        public static IFileCabinetFactory RegisterFileSystemProvider(this IFileCabinetFactory factory) {
            var provider = new FileSystemStorageProvider(fileSystemFactory: () => new System.IO.Abstractions.FileSystem());
            factory.RegisterProvider(provider);
            return factory;
        }
    }
}

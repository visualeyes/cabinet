using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3 {
    public static class FileCabinetFactoryExtensions {

        public static IFileCabinetFactory RegisterS3Provider(this IFileCabinetFactory factory) {
            var provider = new AmazonS3StorageProvider();
            factory.RegisterProvider(provider);
            return factory;
        }
    }
}

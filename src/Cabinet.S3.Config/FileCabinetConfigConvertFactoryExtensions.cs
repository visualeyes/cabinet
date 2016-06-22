using Cabinet.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3.Config {
    public static class FileCabinetConfigConvertFactoryExtensions {

        public static IFileCabinetConfigConverterFactory RegisterAmazonS3ConfigConverter(this IFileCabinetConfigConverterFactory factory) {
            var credentialsFactory = new AWSCredentialsFactory();
            factory.RegisterProvider(AmazonS3CabinetConfig.ProviderType, new AmazonS3ConfigConverter(credentialsFactory));
            return factory;
        }
    }
}

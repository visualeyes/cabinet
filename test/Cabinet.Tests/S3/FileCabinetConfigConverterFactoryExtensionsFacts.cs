using Cabinet.Config;
using Cabinet.Core;
using Cabinet.S3;
using Cabinet.S3.Config;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.S3 {
    public class FileCabinetConfigConverterFactoryExtensionsFacts {

        [Fact]
        public void Registers_Config() {
            var mockFactory = new Mock<IFileCabinetConfigConverterFactory>();

            mockFactory.Object.RegisterAmazonS3ConfigConverter();

            mockFactory.Verify(f => 
                f.RegisterProvider(
                    AmazonS3CabinetConfig.ProviderType,
                    It.IsNotNull<AmazonS3ConfigConverter>()
                ),
                Times.Once
            );
        }
    }
}

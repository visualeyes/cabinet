using Cabinet.Core;
using Cabinet.S3;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.S3 {
    public class FileCabinetFactoryExtensionsFacts {
        [Fact]
        public void Registers_Provider() {
            var mockFactory = new Mock<IFileCabinetFactory>();

            mockFactory.Object.RegisterS3Provider();
            mockFactory.Verify(f => f.RegisterProvider(It.IsNotNull<AmazonS3StorageProvider>()), Times.Once);
        }
    }
}

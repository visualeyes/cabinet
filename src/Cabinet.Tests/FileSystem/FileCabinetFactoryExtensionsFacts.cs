using Cabinet.Core;
using Cabinet.FileSystem;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.FileSystem {
    public class FileCabinetFactoryExtensionsFacts {

        [Fact]
        public void Registers_Provider() {
            var mockFactory = new Mock<IFileCabinetFactory>();
            
            mockFactory.Object.RegisterFileSystemProvider();
            mockFactory.Verify(f => f.RegisterProvider(It.IsNotNull<FileSystemStorageProvider>()), Times.Once);
        }
    }
}

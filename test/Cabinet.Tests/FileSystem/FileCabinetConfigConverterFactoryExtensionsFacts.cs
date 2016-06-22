using Cabinet.Config;
using Cabinet.Core;
using Cabinet.FileSystem;
using Cabinet.FileSystem.Config;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.FileSystem {
    public class FileCabinetConfigConverterFactoryExtensionsFacts {

        [Fact]
        public void Registers_Config() {
            var mockFactory = new Mock<IFileCabinetConfigConverterFactory>();
            var pathMapper = new Mock<IPathMapper>();

            mockFactory.Object.RegisterFileSystemConfigConverter(pathMapper.Object);

            mockFactory.Verify(f => 
                f.RegisterProvider(
                    FileSystemCabinetConfig.ProviderType,
                    It.IsNotNull<FileSystemConfigConverter>()
                ),
                Times.Once
            );
        }
    }
}

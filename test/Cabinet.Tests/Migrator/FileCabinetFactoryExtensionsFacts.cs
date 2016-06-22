using Cabinet.Core;
using Cabinet.Migrator;
using Cabinet.Migrator.Migration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Migrator {
    public class FileCabinetFactoryExtensionsFacts {

        [Fact]
        public void Registers_Provider() {
            var mockFactory = new Mock<IFileCabinetFactory>();
            
            mockFactory.Object.RegisterMigratorProvider();
            mockFactory.Verify(f => f.RegisterProvider(It.IsNotNull<MigrationStorageProvider>()), Times.Once);
        }
    }
}

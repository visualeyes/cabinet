using Cabinet.FileSystem;
using Moq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.FileSystem {
    public class FileCabinetFileInfoFacts {

        [Fact]
        public void Null_File_Info_Throws() {
            Assert.Throws<ArgumentNullException>(() => new FileSystemCabinetFileInfo(null, @"C:\test"));
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData("  ")]
        public void Null_Base_Dir_Throws(string baseDirectory) {
            var mockFileInfo = new Mock<FileInfoBase>();
            Assert.Throws<ArgumentNullException>(() => new FileSystemCabinetFileInfo(mockFileInfo.Object, baseDirectory));
        }
    }
}

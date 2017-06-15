using Cabinet.FileSystem;
using System;
using System.IO;
using Xunit;

namespace Cabinet.Tests.FileSystem {
    public class FileSystemCabinetConfigFacts {

        [Theory]
        [InlineData(null), InlineData(""), InlineData("  ")]
        public void Null_Or_Empty_Directory_Throws(string dir) {
            Assert.Throws<ArgumentNullException>(() => new FileSystemCabinetConfig(dir));
        }

        [Fact]
        public void Delimiter_Is_DirectorySeparatorChar() {
            string dir = "C:\\test\\file.txt";
            var config = new FileSystemCabinetConfig(dir);
            Assert.Equal(Path.DirectorySeparatorChar.ToString(), config.Delimiter);
        }
    }
}

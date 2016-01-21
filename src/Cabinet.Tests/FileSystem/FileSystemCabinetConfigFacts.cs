using Cabinet.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.FileSystem {
    public class FileSystemCabinetConfigFacts {

        [Theory]
        [InlineData(null), InlineData(""), InlineData("  ")]
        public void Null_Or_Empty_Directory_Throws(string dir) {
            Assert.Throws<ArgumentNullException>(() => new FileSystemCabinetConfig(dir));
        }
    }
}

using Cabinet.FileSystem.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.FileSystem.Results {
    public class SaveResultFacts {
        [Theory]
        [InlineData(true), InlineData(false)]
        public void Sets_Success(bool success) {
            var result = new SaveResult(success);
            Assert.Equal(success, result.Success);
        }

        [Fact]
        public void Sets_Exception() {
            var exception = new Exception("Test");
            var result = new SaveResult(exception);

            Assert.False(result.Success);
            Assert.Equal(exception, result.Exception);
        }
    }
}

using Cabinet.FileSystem.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.FileSystem.Results {
    public class DeleteResultFacts {

        [Theory]
        [InlineData(true), InlineData(false)]
        public void Sets_Success(bool success) {
            var result = new DeleteResult(success);
            Assert.Equal(success, result.Success);
        }

        [Fact]
        public void Null_Exception_Throws() {
            Assert.Throws<ArgumentNullException>(() => new DeleteResult(null));
        }

        [Fact]
        public void Sets_Exception() {
            var exception = new Exception("Test");
            var result = new DeleteResult(exception);

            Assert.False(result.Success);
            Assert.Equal(exception, result.Exception);
        }

        [Theory]
        [MemberData("GetExceptionMessages")]
        public void Get_Exception_Message(Exception e, string msg) {
            var result = new DeleteResult(e);
            Assert.Equal(msg, result.GetErrorMessage());
        }

        public static object[][] GetExceptionMessages() {
            return new [] {
                new object[] { new DirectoryNotFoundException(), "Could not find the file" },
                new object[] { new PathTooLongException(), "The path is too long. The path must be less than 248 characters and file name less than 260 characters." },
                new object[] { new UnauthorizedAccessException(), "Could not delete the file" },
                new object[] { new NotSupportedException(), "The path is not valid" },
                new object[] { new IOException(), "The file is in use, is read-only or is a directory" },
                new object[] { new Exception(), null },
            };
        }
    }
}

using Cabinet.FileSystem.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.FileSystem.Results {
    public class MoveResultFacts {
        [Theory]
        [InlineData(true), InlineData(false)]
        public void Sets_Success(bool success) {
            var result = new MoveResult(success);
            Assert.Equal(success, result.Success);
        }

        [Fact]
        public void Null_Exception_Throws() {
            Assert.Throws<ArgumentNullException>(() => new MoveResult(null));
        }

        [Fact]
        public void Sets_Exception() {
            var exception = new Exception("Test");
            var result = new MoveResult(exception);

            Assert.False(result.Success);
            Assert.Equal(exception, result.Exception);
        }

        [Theory]
        [InlineData(""), InlineData("test")]
        public void Sets_ErrorMsg(string msg) {
            var exception = new Exception("Test");
            var result = new MoveResult(exception, errorMsg: msg);

            Assert.Equal(msg, result.GetErrorMessage());
        }

        [Theory]
        [MemberData("GetExceptionMessages")]
        public void Get_Exception_Message(Exception e, string msg) {
            var result = new MoveResult(e);
            Assert.Equal(msg, result.GetErrorMessage());
        }

        public static object[] GetExceptionMessages() {
            return new object[] {
                new object[] { new UnauthorizedAccessException(), "Could not move the file" },
                new object[] { new PathTooLongException(), "The path is too long. The path must be less than 248 characters and file name less than 260 characters." },
                new object[] { new DirectoryNotFoundException(), "The source or destination directory could not be found" },
                new object[] { new NotSupportedException(), "The source or destination name is not valid" },
                new object[] { new IOException(), "Destination file already exists" },
                new object[] { new Exception(), null },
            };
        }
    }
}

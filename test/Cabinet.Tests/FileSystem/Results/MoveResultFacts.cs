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
        [InlineData("sourceKey", "destKey", true), InlineData("sourceKey", "destKey", false)]
        public void Sets_Success(string sourceKey, string destKey, bool success) {
            var result = new MoveResult(sourceKey, destKey, success);
            Assert.Equal(success, result.Success);
        }

        [Fact]
        public void Null_Exception_Throws() {
            Exception e = null;
            Assert.Throws<ArgumentNullException>(() => new MoveResult("sourceKey", "destKey", e));
        }

        [Fact]
        public void Sets_Exception() {
            var exception = new Exception("Test");
            var result = new MoveResult("sourceKey", "destKey", exception);

            Assert.False(result.Success);
            Assert.Equal(exception, result.Exception);
        }

        [Theory]
        [InlineData(""), InlineData("test")]
        public void Sets_ErrorMsg(string msg) {
            var exception = new Exception("Test");
            var result = new MoveResult("sourceKey", "destKey", exception, errorMsg: msg);

            Assert.Equal(msg, result.GetErrorMessage());
        }

        [Theory]
        [MemberData("GetExceptionMessages")]
        public void Get_Exception_Message(Exception e, string msg) {
            var result = new MoveResult("sourceKey", "destKey", e);
            Assert.Equal(msg, result.GetErrorMessage());
        }

        public static object[][] GetExceptionMessages() {
            return new [] {
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

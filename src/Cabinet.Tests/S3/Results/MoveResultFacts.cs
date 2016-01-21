using Cabinet.S3.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.S3.Results {
    public class MoveResultFacts {
        [Theory]
        [InlineData(true), InlineData(false)]
        public void Sets_Success(bool success) {
            var result = new MoveResult(success);
            Assert.Equal(success, result.Success);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK, true, null)]
        [InlineData(HttpStatusCode.NotFound, false, "File does not exist")]
        [InlineData(HttpStatusCode.Forbidden, false, null)]
        [InlineData(HttpStatusCode.Unauthorized, false, "Access to the bucket is denied")]
        [InlineData(HttpStatusCode.InternalServerError, false, null)]
        public void Sets_Success(HttpStatusCode code, bool success, string errorMsg) {
            var result = new MoveResult(code);
            Assert.Equal(success, result.Success);
            Assert.Equal(errorMsg, result.GetErrorMessage());
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
                new object[] { new Exception("test"), "test" },
            };
        }
    }
}

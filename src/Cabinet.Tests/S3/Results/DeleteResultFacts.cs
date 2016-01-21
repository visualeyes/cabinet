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
    public class DeleteResultFacts {

        [Theory]
        [InlineData(HttpStatusCode.OK, true)]
        [InlineData(HttpStatusCode.NotFound, false)]
        [InlineData(HttpStatusCode.Forbidden, false)]
        [InlineData(HttpStatusCode.InternalServerError, false)]
        public void Sets_Success(HttpStatusCode code, bool success) {
            var result = new DeleteResult(code);
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

        public static object[] GetExceptionMessages() {
            return new object[] {
                new object[] { new Exception(), null },
            };
        }
    }
}

using Cabinet.Migrator.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Migrator.Results {
    public class MoveResultFacts {
       
        [Theory]
        [InlineData(true, null)]
        [InlineData(false, null)]
        [InlineData(false, "File does not exist")]
        public void Sets_Success(bool success, string errorMsg) {
            var result = new MoveResult("sourceKey", "destKey", success: success, errorMsg: errorMsg);
            Assert.Equal(success, result.Success);
            Assert.Equal(errorMsg, result.GetErrorMessage());
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

        public static object[] GetExceptionMessages() {
            return new object[] {
                new object[] { new Exception("test"), "System.Exception: test" },
            };
        }
    }
}

using Cabinet.Core.Results;
using Cabinet.Web.Results;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Web.Results {
    public class UploadSaveResultFacts {

        [Theory]
        [InlineData(null, "text/plain", "error"), InlineData("", "text/plain", "error"), InlineData("  ", "text/plain", "error")]
        [InlineData("blah.txt", null, "error"), InlineData("blah.txt", "", "error"), InlineData("blah.txt", "  ", "error")]
        [InlineData("blah.txt", "text/plain", null), InlineData("blah.txt", "text/plain", ""), InlineData("blah.txt", "text/plain", "  ")]
        public void NullEmpty_Error_Constructor_Throws(string uploadFileName, string uploadFileMediaType, string errorMessage) {
            Assert.Throws<ArgumentNullException>(() => new UploadSaveResult(uploadFileName, uploadFileMediaType, errorMessage));
        }

        [Theory]
        [InlineData(null, "text/plain"), InlineData("", "text/plain"), InlineData("  ", "text/plain")]
        [InlineData("blah.txt", null), InlineData("blah.txt", ""), InlineData("blah.txt", "  ")]
        public void NullEmpty_Result_Constructor_Throws(string uploadFileName, string uploadFileMediaType) {
            var saveResult = new Mock<ISaveResult>();
            Assert.Throws<ArgumentNullException>(() => new UploadSaveResult(uploadFileName, uploadFileMediaType, saveResult.Object));
        }

        [Fact]
        public void Null_Result_Constructor_Throws() {
            ISaveResult saveResult = null;
            Assert.Throws<ArgumentNullException>(() => new UploadSaveResult("blah.txt", "text/plain", saveResult));
        }

        [Theory]
        [InlineData("blah.txt", "text/plain", "error")]
        public void Error_Result_Set(string uploadFileName, string uploadFileMediaType, string errorMessage) {
            var result = new UploadSaveResult(uploadFileName, uploadFileMediaType, errorMessage);

            Assert.False(result.Success);
            Assert.False(result.AlreadyExists);
            Assert.Equal(uploadFileName, result.UploadFileName);
            Assert.Equal(uploadFileMediaType, result.UploadFileMediaType);
            Assert.Equal(errorMessage, result.GetErrorMessage());
        }

        [Theory]
        [InlineData("blah.txt", "text/plain", "key", true, false, null)]
        [InlineData("blah.txt", "text/plain", "key", false, false, "error")]
        public void Result_Set(string uploadFileName, string uploadFileMediaType, string key, bool success, bool alreadyExists, string errorMessage) {
            var saveResult = new Mock<ISaveResult>();
            saveResult.SetupGet(r => r.Key).Returns(key);
            saveResult.SetupGet(r => r.Success).Returns(success);
            saveResult.SetupGet(r => r.AlreadyExists).Returns(alreadyExists);
            saveResult.Setup(r => r.GetErrorMessage()).Returns(errorMessage);

            var result = new UploadSaveResult(uploadFileName, uploadFileMediaType, saveResult.Object);

            Assert.Equal(key, result.Key);
            Assert.Equal(success, result.Success);
            Assert.Equal(alreadyExists, result.AlreadyExists);

            Assert.Equal(uploadFileName, result.UploadFileName);
            Assert.Equal(uploadFileMediaType, result.UploadFileMediaType);
            Assert.Equal(errorMessage, result.GetErrorMessage());
        }
    }
}

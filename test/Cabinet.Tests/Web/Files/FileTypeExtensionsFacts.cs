using Cabinet.Web.Files;
using System.Collections.Generic;
using Xunit;

namespace Cabinet.Tests.Web.Files {
    public class FileTypeExtensionsFacts {

        IEnumerable<IFileType> fileTypes;

        public FileTypeExtensionsFacts() {
            fileTypes = new FileTypeProvider().GetFileTypes();
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("exe", null)]
        [InlineData("jpg", "image/jpeg")]
        [InlineData("JPG", "image/jpeg")]
        public void GetByExtension_Theory(string ext, string expectedMimeType) {
            var result = fileTypes.GetByExtension(ext);

            if(expectedMimeType == null) {
                Assert.Null(result);
            } else {
                Assert.Equal(expectedMimeType, result.MimeType);
            }
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("application/octet-stream", null)]
        [InlineData("image/jpeg", "image/jpeg")]
        [InlineData("iMaGe/jPeG", "image/jpeg")]
        [InlineData("application/pjpeg", "image/jpeg")]
        [InlineData("aPplIcAtIon/pJpEg", "image/jpeg")]
        public void GetByMimeType_Theory(string mimeType, string expectedMimeType) {
            var result = fileTypes.GetByMimeType(mimeType);

            if(expectedMimeType == null) {
                Assert.Null(result);
            } else {
                Assert.Equal(expectedMimeType, result.MimeType);
            }
        }
    }
}

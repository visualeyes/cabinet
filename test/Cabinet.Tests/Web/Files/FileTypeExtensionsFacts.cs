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
        [InlineData("", null)]
        [InlineData("invalid file path", null)]
        [InlineData(".jpg", "image/jpeg")]
        [InlineData("image.jpg", "image/jpeg")]
        [InlineData("folder/image.jpg", "image/jpeg")]
        public void GetByFilePath(string filePath, string expectedMimeType) {
            AssertEqualMimeType(expectedMimeType, fileTypes.GetByFilePath(filePath));
        }

        [Theory]
        [InlineData("", null)]
        [InlineData("exe", null)]
        [InlineData("jpg", "image/jpeg")]
        [InlineData(".jpg", "image/jpeg")]
        [InlineData("JPG", "image/jpeg")]
        public void GetByExtension_Theory(string ext, string expectedMimeType) {
            AssertEqualMimeType(expectedMimeType, fileTypes.GetByExtension(ext));
        }

        [Theory]
        [InlineData("", null)]
        [InlineData("application/octet-stream", null)]
        [InlineData("image/jpeg", "image/jpeg")]
        [InlineData("iMaGe/jPeG", "image/jpeg")]
        [InlineData("application/pjpeg", "image/jpeg")]
        [InlineData("aPplIcAtIon/pJpEg", "image/jpeg")]
        public void GetByMimeType_Theory(string mimeType, string expectedMimeType) {
            AssertEqualMimeType(expectedMimeType, fileTypes.GetByMimeType(mimeType));
        }

        private void AssertEqualMimeType(string expectedMimeType, IFileType result) {
            if(expectedMimeType == null) {
                Assert.Null(result);
            } else {
                Assert.Equal(expectedMimeType, result.MimeType);
            }
        }
    }
}

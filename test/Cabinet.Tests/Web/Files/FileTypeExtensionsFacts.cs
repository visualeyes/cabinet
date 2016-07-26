using Cabinet.Web.Files;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Web.Files {
    public class FileTypeExtensionsFacts {
        [Theory]
        [InlineData("jpg", true), InlineData("jPg", true), InlineData("JPG", true)]
        [InlineData("exe", false), InlineData("eXe", false), InlineData("EXE", false)]
        public void Case_Insenstive_FileType_Check(string ext, bool allowed) {
            var mockFileType = new Mock<IFileType>();
            mockFileType.SetupGet(ft => ft.Extensions).Returns(new string[] { "jpg" });

            Assert.Equal(allowed, mockFileType.Object.IsValidExtension(ext));
        }
    }
}

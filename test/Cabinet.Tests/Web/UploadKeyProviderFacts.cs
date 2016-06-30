using Cabinet.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Web {
    public class UploadKeyProviderFacts {
        private const string GuidRegex = @"[{(]?[0-9a-fA-F]{8}[-]?([0-9a-fA-F]{4}[-]?){3}[0-9a-fA-F]{12}[)}]?";

        private readonly UploadKeyProvider provider;

        public UploadKeyProviderFacts() {
            this.provider = new UploadKeyProvider();
        }

        [Theory]
        [InlineData("blah.jpg", null), InlineData("blah.jpg", ""), InlineData("blah.jpg", "  ")]
        [InlineData(null, "image/jpeg"), InlineData("", "image/jpeg"), InlineData("  ", "image/jpeg")]
        public void GetKey_Throws(string fileName, string contentType) {
            Assert.Throws<ArgumentNullException>(() => provider.GetKey(fileName, contentType, "/"));
        }
        
        [Theory]
        [InlineData("blah.jpg", "/", "blah.jpg", "image/jpeg")]
        public void GetKey(string fileName, string delimiter, string expectedFileName, string contentType) {
            string key = provider.GetKey(fileName, contentType, delimiter);
            Assert.Matches(GuidRegex + delimiter + expectedFileName + ".upload", key);
        }

        [Fact]
        public void Get_Long_Key() {
            string expectedFileName = new string('a', 241);
            expectedFileName += "---.jpg.upload";

            string fileName = new string('a', 300);
            fileName += ".jpg";

            string key = provider.GetKey(fileName, "image/jpeg", "/");

            Assert.NotNull(key);

            string actualFileName = Path.GetFileName(key);
            Assert.Equal(expectedFileName, actualFileName);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData("  ")]
        public void NormalizeKey_Throws(string key) {
            Assert.Throws<ArgumentNullException>(() => provider.NormalizeKey(key));
        }

        [Theory]
        [InlineData("bb38c5af-477a-4178-836c-17b445f733c7\blah.jpg.upload", "bb38c5af-477a-4178-836c-17b445f733c7\blah.jpg")]
        [InlineData("bb38c5af-477a-4178-836c-17b445f733c7\blah.jpg", "bb38c5af-477a-4178-836c-17b445f733c7\blah.jpg")]
        public void NormalizeKey(string key, string expected) {
            string normalisedKey = provider.NormalizeKey(key);
            Assert.Equal(expected, normalisedKey);
        }
    }
}

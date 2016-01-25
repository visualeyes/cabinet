using Cabinet.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.S3 {
    public class S3CabinetFileInfoFacts {

        [Fact]
        public void Provider_Type() {
            var fileInfo = new S3CabinetFileInfo("key", true);
            Assert.Equal(AmazonS3StorageProvider.ProviderType, fileInfo.ProviderType);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public void Null_Key_Throws(string key) {
            Assert.Throws<ArgumentNullException>(() => new S3CabinetFileInfo(key, true));
        }

        [Fact]
        public void Key_Set() {
            string key = "key";
            var fileInfo = new S3CabinetFileInfo(key, true);
            Assert.Equal(key, fileInfo.Key);
        }

        [Theory]
        [InlineData(true), InlineData(false)]
        public void Exists_Set(bool exists) {
            string key = "key";
            var fileInfo = new S3CabinetFileInfo(key, exists);
            Assert.Equal(exists, fileInfo.Exists);
        }
    }
}

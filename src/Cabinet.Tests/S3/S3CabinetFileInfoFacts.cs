using Cabinet.Core;
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
            var fileInfo = new AmazonS3CabinetItemInfo("key", true, ItemType.File);
            Assert.Equal(AmazonS3CabinetConfig.ProviderType, fileInfo.ProviderType);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public void Null_Key_Throws(string key) {
            Assert.Throws<ArgumentNullException>(() => new AmazonS3CabinetItemInfo(key, true, ItemType.File));
        }

        [Fact]
        public void Key_Set() {
            string key = "key";
            var fileInfo = new AmazonS3CabinetItemInfo(key, true, ItemType.File);
            Assert.Equal(key, fileInfo.Key);
        }

        [Theory]
        [InlineData(true), InlineData(false)]
        public void Exists_Set(bool exists) {
            string key = "key";
            var fileInfo = new AmazonS3CabinetItemInfo(key, exists, ItemType.File);
            Assert.Equal(exists, fileInfo.Exists);
        }
    }
}

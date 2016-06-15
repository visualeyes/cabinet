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
        private const string ValidKey = "key";

        [Fact]
        public void Provider_Type() {
            var fileInfo = new AmazonS3CabinetItemInfo(ValidKey, true, ItemType.File, null);
            Assert.Equal(AmazonS3CabinetConfig.ProviderType, fileInfo.ProviderType);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public void Null_Key_Throws(string key) {
            Assert.Throws<ArgumentNullException>(() => new AmazonS3CabinetItemInfo(key, true, ItemType.File, null));
        }

        [Fact]
        public void Key_Set() {
            var fileInfo = new AmazonS3CabinetItemInfo(ValidKey, true, ItemType.File, null);
            Assert.Equal(ValidKey, fileInfo.Key);
        }

        [Theory]
        [InlineData(true), InlineData(false)]
        public void Exists_Set(bool exists) {
            var fileInfo = new AmazonS3CabinetItemInfo(ValidKey, exists, ItemType.File, null);
            Assert.Equal(exists, fileInfo.Exists);
        }

        [Theory]
        [InlineData(null), InlineData("20-Feb-2016")]
        public void LastModified_Set(string date) {
            DateTime? lastModified = String.IsNullOrWhiteSpace(date) ? null : (DateTime?)DateTime.Parse(date);

            var fileInfo = new AmazonS3CabinetItemInfo(ValidKey, true, ItemType.File, lastModified);
            Assert.Equal(lastModified, fileInfo.LastModifiedUtc);
        }
    }
}

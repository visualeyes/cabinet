using Cabinet.Core;
using Cabinet.S3;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.S3 {
    public class AmazonS3CabinetItemInfoComparerFacts {
        private readonly CabinetItemInfoKeyComparer comparer;

        public AmazonS3CabinetItemInfoComparerFacts() {
            this.comparer = new CabinetItemInfoKeyComparer();
        }

        [Theory]
        [InlineData("key", ItemType.File, "key", ItemType.File, true)]
        [InlineData("key", ItemType.File, "key", ItemType.Directory, true)]
        [InlineData("key", ItemType.File, "key2", ItemType.File, false)]
        public void Equals_Amazon_Type(string key1, ItemType itemType1, string key2, ItemType itemType2, bool expectedExists) {
            var item1 = GetAmazonItemInfo(key1, itemType1);
            var item2 = GetAmazonItemInfo(key2, itemType2);

            bool exists = this.comparer.Equals(item1, item2);

            Assert.Equal(expectedExists, exists);
        }

        [Theory]
        [InlineData("type1", "key", ItemType.File, "type1", "key", ItemType.File, true)]
        [InlineData("type1", "key", ItemType.File, "type1", "key", ItemType.Directory, true)]
        [InlineData("type1", "key", ItemType.File, "type1", "key2", ItemType.File, false)]
        [InlineData("type1", "key", ItemType.File, "type2", "key", ItemType.Directory, false)]
        public void Equals_Item_Type(string providerType1, string key1, ItemType itemType1, string providerType2, string key2, ItemType itemType2, bool expectedExists) {
            var item1 = GetICabinetItemInfo(providerType1, key1, itemType1);
            var item2 = GetICabinetItemInfo(providerType2, key2, itemType2);

            bool exists = this.comparer.Equals(item1, item2);

            Assert.Equal(expectedExists, exists);
        }

        [Fact]
        public void Hashcode_Amazon_Type() {
            var item = GetAmazonItemInfo("key", ItemType.File);
            this.comparer.GetHashCode(item);
        }

        [Fact]
        public void Hashcode_Item_Type() {
            var item = GetICabinetItemInfo("type1", "key", ItemType.File);
            this.comparer.GetHashCode(item);
        }

        private static ICabinetItemInfo GetICabinetItemInfo(string providerType, string key, ItemType itemType) {
            var mockItem = new Mock<ICabinetItemInfo>();
            mockItem.SetupGet(i => i.ProviderType).Returns(providerType);
            mockItem.SetupGet(i => i.Key).Returns(key);
            mockItem.SetupGet(i => i.Type).Returns(itemType);

            return mockItem.Object;
        }

        private static AmazonS3CabinetItemInfo GetAmazonItemInfo(string key1, ItemType itemType1) {
            return new AmazonS3CabinetItemInfo(key1, true, itemType1, null);
        }
    }
}

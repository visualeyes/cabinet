using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Core {
    public class KeyUtilsFacts {

        [Theory]
        [InlineData("", "", "/", ""), InlineData(null, null, "/", "")]
        [InlineData("", "test.txt", "/", "test.txt"), InlineData(null, "folder/test.txt", "/", "folder/test.txt")]
        [InlineData("folder", "", "/", "folder"), InlineData("folder", null, "/", "folder")]
        [InlineData("prefix", "test.txt", "/", "prefix/test.txt"), InlineData("prefix", "folder/test.txt", "/", "prefix/folder/test.txt")]
        [InlineData("prefix/", "/test.txt", "/", "prefix/test.txt"), InlineData("prefix/", "/folder/test.txt", "/", "prefix/folder/test.txt")]
        public void Join_Keys(string prefix, string key, string delimiter, string expected) {
            string actual = KeyUtils.JoinKeys(prefix, key, delimiter);

            Assert.Equal(expected, actual);
        }
    }
}

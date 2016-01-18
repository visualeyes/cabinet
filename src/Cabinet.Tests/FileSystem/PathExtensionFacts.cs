using Cabinet.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.FileSystem {
    public class PathExtensionFacts {

        [Theory]
        [MemberData("GetTestPaths")]
        public void Is_SameDirectory(string subPath, string basePath, bool isChildOf, bool isSameDir) {
            var actualResult = subPath.IsSameDirectory(basePath);

            Assert.Equal(isSameDir, actualResult);
        }

        [Theory]
        [MemberData("GetTestPaths")]
        public void Is_SubDirectory_Of(string subPath, string basePath, bool isChildOf, bool isSameDir) {
            var actualResult = subPath.IsSubDirectoryOf(basePath);

            Assert.Equal(isChildOf, actualResult);
        }

        public static object[] GetTestPaths() {
            return new object[] {
                new object[] { @"c:\foo", @"c:", false, false },
                new object[] { @"c:\foo", @"c:\", true, false },
                new object[] { @"c:\foo", @"c:\foo", false, true },
                new object[] { @"c:\foo", @"c:\foo\", false, true },
                new object[] { @"c:\foo\", @"c:\foo", false, true },
                new object[] { @"c:\FOO\", @"c:\foo", false, true },
                new object[] { @"c:\foo\bar\", @"c:\foo\", true, false },
                new object[] { @"c:\FOO\bar\", @"c:\foo\", true, false },
                new object[] { @"c:\foo\bar", @"c:\foo\", true, false },
                new object[] { @"c:\foo\a.txt", @"c:\foo", true, false },
                new object[] { @"c:\FOO\a.txt", @"c:\foo", true, false },
                new object[] { @"c:/foo/a.txt", @"c:\foo", true, false },
                new object[] { @"c:\foobar", @"c:\foo", false, false },
                new object[] { @"c:\foobar\a.txt", @"c:\foo", false, false },
                new object[] { @"c:\foobar\a.txt", @"c:\foo\", false, false },
                new object[] { @"c:\foo\a.txt", @"c:\foobar", false, false },
                new object[] { @"c:\foo\a.txt", @"c:\foobar\", false, false },
                new object[] { @"c:\foo\..\bar\baz", @"c:\foo", false, false },
                new object[] { @"c:\foo\..\bar\baz", @"c:\bar", true, false },
                new object[] { @"c:\foo\..\bar\baz", @"c:\barr", false, false },
            };
        }
    }
}

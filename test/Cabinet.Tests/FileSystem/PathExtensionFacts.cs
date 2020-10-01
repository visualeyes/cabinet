using Cabinet.FileSystem;
using Moq;
using System;
using System.IO.Abstractions;
using Xunit;

namespace Cabinet.Tests.FileSystem {
    public class PathExtensionFacts {

        [Theory]
        [InlineData(@"c:\foo", @"C:\foo\bar", @"bar")]
        [InlineData(@"c:\foo\", @"C:\foo\bar", @"bar")]
        [InlineData(@"c:\foo\", @"C:\foo\bar\baz.txt", @"bar\baz.txt")]
        [InlineData(@"c:\foo\", @"C:\foo\foo bar\baz.txt", @"foo bar\baz.txt")]
        public void MakeRelativeTo(string basePath, string subPath, string expectedRelativePath) {
            string actual = subPath.MakeRelativeTo(basePath);
            Assert.Equal(expectedRelativePath, actual);
        }

        [Theory]
        [InlineData(null, @"C:\foo")]
        [InlineData("", @"C:\foo")]
        [InlineData("   ", @"C:\foo")]
        [InlineData(@"C:\foo", null)]
        [InlineData(@"C:\foo", "")]
        [InlineData(@"C:\foo", "   ")]
        public void Is_SameDirectory_Throws_If_Null_Or_Empty_Path(string path1, string path2) {
            Assert.Throws<ArgumentNullException>(() => {
                path1.IsSameDirectory(path2);
            });
        }

        [Fact]
        public void Is_SameDirectory_Throws_If_Null_Dir1() {
            DirectoryInfoBase item1 = null;
            DirectoryInfoBase item2 = new Mock<DirectoryInfoBase>().Object;

            Assert.Throws<ArgumentNullException>(() => {
                item1.IsSameDirectory(item2);
            });
        }

        [Fact]
        public void Is_SameDirectory_Throws_If_Null_Dir2() {
            DirectoryInfoBase item1 = new Mock<DirectoryInfoBase>().Object;
            DirectoryInfoBase item2 = null;

            Assert.Throws<ArgumentNullException>(() => {
                item1.IsSameDirectory(item2);
            });
        }

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

        public static object[][] GetTestPaths() {
            return new [] {
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

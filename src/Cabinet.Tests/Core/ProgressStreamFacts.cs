using Cabinet.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Core {
    public class ProgressStreamFacts {

        [Fact]
        public void Null_Inner_Stream_Throws() {
            var mockProgress = new Mock<IProgress<WriteProgress>>();
            Assert.Throws<ArgumentNullException>(() => new ProgressStream(null, 1, mockProgress.Object));
        }

        [Theory]
        [InlineData(true), InlineData(false)]
        public void Inner_Stream_Calls_CanRead(bool canRead) {
            var mockStream = new Mock<Stream>();
            var mockProgress = new Mock<IProgress<WriteProgress>>();
            var progressStream = new ProgressStream(mockStream.Object, null, null);

            mockStream.SetupGet(s => s.CanRead).Returns(canRead);

            bool actualCanRead = progressStream.CanRead;

            mockStream.Verify(s => s.CanRead, Times.Once);

            Assert.Equal(canRead, actualCanRead);
        }

        [Theory]
        [InlineData(true), InlineData(false)]
        public void Inner_Stream_Calls_CanWrite(bool canWrite) {
            var mockStream = new Mock<Stream>();
            var mockProgress = new Mock<IProgress<WriteProgress>>();
            var progressStream = new ProgressStream(mockStream.Object, null, null);

            mockStream.SetupGet(s => s.CanWrite).Returns(canWrite);

            bool actualCanWrite = progressStream.CanWrite;

            mockStream.Verify(s => s.CanWrite, Times.Once);

            Assert.Equal(canWrite, actualCanWrite);
        }

        [Theory]
        [InlineData(true), InlineData(false)]
        public void Inner_Stream_Calls_CanSeek(bool canSeek) {
            var mockStream = new Mock<Stream>();
            var mockProgress = new Mock<IProgress<WriteProgress>>();
            var progressStream = new ProgressStream(mockStream.Object, null, null);

            mockStream.SetupGet(s => s.CanWrite).Returns(canSeek);

            bool actualCanSeek = progressStream.CanWrite;

            mockStream.Verify(s => s.CanWrite, Times.Once);

            Assert.Equal(canSeek, actualCanSeek);
        }
    }
}

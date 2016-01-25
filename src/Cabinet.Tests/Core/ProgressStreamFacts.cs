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
            var progressStream = new ProgressStream(mockStream.Object, null, null);

            mockStream.SetupGet(s => s.CanWrite).Returns(canSeek);

            bool actualCanSeek = progressStream.CanWrite;

            mockStream.Verify(s => s.CanWrite, Times.Once);

            Assert.Equal(canSeek, actualCanSeek);
        }

        [Theory]
        [InlineData(0), InlineData(10)]
        public void Inner_Stream_Calls_Length(long length) {
            var mockStream = new Mock<Stream>();
            var progressStream = new ProgressStream(mockStream.Object, null, null);

            mockStream.SetupGet(s => s.Length).Returns(length);

            long actualLength = progressStream.Length;

            mockStream.Verify(s => s.Length, Times.Once);

            Assert.Equal(length, actualLength);
        }

        [Theory]
        [InlineData(0), InlineData(10)]
        public void Inner_Stream_Calls_Position(long position) {
            var mockStream = new Mock<Stream>();
            var progressStream = new ProgressStream(mockStream.Object, null, null);

            mockStream.SetupGet(s => s.Position).Returns(position);

            long actualPosition = progressStream.Position;

            mockStream.Verify(s => s.Position, Times.Once);

            Assert.Equal(position, actualPosition);
        }

        [Fact]
        public void Inner_Stream_Calls_Flush() {
            var mockStream = new Mock<Stream>();
            var progressStream = new ProgressStream(mockStream.Object, null, null);

            progressStream.Flush();

            mockStream.Verify(s => s.Flush(), Times.Once);
        }

        [Fact]
        public void Inner_Stream_Calls_Read() {
            var mockStream = new Mock<Stream>();
            var progressStream = new ProgressStream(mockStream.Object, null, null);

            byte[] buffer = new byte[0];
            int offset = 0;
            int count = 0;

            progressStream.Read(buffer, offset, count);

            mockStream.Verify(s => s.Read(buffer, offset, count), Times.Once);
        }

        [Fact]
        public void Inner_Stream_Calls_Seek() {
            var mockStream = new Mock<Stream>();
            var progressStream = new ProgressStream(mockStream.Object, null, null);

            long offset = 0;
            SeekOrigin origin = SeekOrigin.Begin;

            progressStream.Seek(offset, origin);

            mockStream.Verify(s => s.Seek(offset, origin), Times.Once);
        }

        [Fact]
        public void Inner_Stream_Calls_SetLength() {
            var mockStream = new Mock<Stream>();
            var progressStream = new ProgressStream(mockStream.Object, null, null);

            long length = 0;

            progressStream.SetLength(length);

            mockStream.Verify(s => s.SetLength(length), Times.Once);
        }

        [Fact]
        public void Inner_Stream_Calls_Write() {
            var mockStream = new Mock<Stream>();
            var mockProgress = new Mock<IProgress<WriteProgress>>();
            var progressStream = new ProgressStream(mockStream.Object, null, mockProgress.Object);

            byte[] buffer = new byte[0];
            int offset = 0;
            int count = 0;

            mockProgress.Setup(p => p.Report(It.IsAny<WriteProgress>()));

            progressStream.Write(buffer, offset, count);

            mockProgress.Verify(p => p.Report(It.IsAny<WriteProgress>()), Times.Once);
            mockStream.Verify(s => s.Write(buffer, offset, count), Times.Once);
        }

        [Theory]
        [InlineData(true), InlineData(false)]
        public void Inner_Stream_Calls_Dispose(bool disposeStream) {
            var mockStream = new Mock<Stream>();
            var mockProgress = new Mock<IProgress<WriteProgress>>();
            var progressStream = new ProgressStream(mockStream.Object, null, mockProgress.Object, disposeStream);

            progressStream.Dispose();
        }
    }
}

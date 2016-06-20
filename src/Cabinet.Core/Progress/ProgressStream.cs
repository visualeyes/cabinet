using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Core {
    public class ProgressStream : Stream {
        private readonly string key;
        private readonly Stream stream;
        private readonly long? size;

        private readonly IProgress<IWriteProgress> writeProgress;
        private readonly bool disposeStream;

        private long bytesWrittenCount;

        public ProgressStream(string key, Stream stream, long? size, IProgress<IWriteProgress> writeProgress, bool disposeStream = false) {
            Contract.NotNullOrEmpty(key, nameof(key));
            Contract.NotNull(stream, nameof(stream));

            this.key = key;
            this.stream = stream;
            this.size = size;
            this.bytesWrittenCount = 0;
            this.writeProgress = writeProgress;
            this.disposeStream = disposeStream;
        }

        #region Stream methods
        public override bool CanRead {
            get { return stream.CanRead; }
        }

        public override bool CanSeek {
            get { return stream.CanSeek; }
        }

        public override bool CanWrite {
            get { return stream.CanWrite; }
        }

        public override long Length {
            get { return stream.Length; }
        }

        public override long Position {
            get { return stream.Position; }
            set { stream.Position = value; }
        }

        public override void Flush() {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            bytesWrittenCount += count;

            writeProgress?.Report(new WriteProgress(key, bytesWrittenCount, size));
            stream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing) {
            if (disposeStream) {
                stream.Dispose();
            }
        }
        #endregion
    }
}

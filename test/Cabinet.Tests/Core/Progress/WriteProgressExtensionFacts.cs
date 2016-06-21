using Cabinet.Core;
using Cabinet.Core.Progress;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Core.Progress {
    public class WriteProgressExtensionFacts {
        
        [Theory]
        [InlineData(null)]
        [InlineData(10)]
        public void TryGetStreamLength(int? length) {
            var stream = new Mock<Stream>();
            
            if(length.HasValue) {
                stream.SetupGet(s => s.Length).Returns(length.Value);
            } else {
                stream.SetupGet(s => s.Length).Throws<NotSupportedException>();
            }

            Assert.Equal(length, stream.Object.TryGetStreamLength());
        }

        [Theory]
        [MemberData("GetProgressData")]
        public void ProgressPercentage(long bytesWritten, long? totalBytes, double? expected) {
            var progress = new WriteProgress("key", bytesWritten, totalBytes);

            var percentage = progress.ProgressPercentage();

            Assert.Equal(expected, percentage);
        }

        public static object[] GetProgressData() {
            return new object[] {
                new object[] { 50, (long?)0, null },
                new object[] { 100, (long?)50, (double?)2 },
                new object[] { 50, (long?)100, (double?)0.5 },
                new object[] { 100, null, null },
            };
        }
    }
}

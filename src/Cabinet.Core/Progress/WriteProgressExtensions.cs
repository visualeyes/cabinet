using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cabinet.Core.Progress {
    public static class WriteProgressExtensions {
        public static long? TryGetStreamLength(this Stream stream) {
            try {
                return stream.Length;
            } catch(NotSupportedException) {
                return null;
            }
        }

        public static double? ProgressPercentage(this IWriteProgress progress) {
            if(!progress.TotalBytes.HasValue) {
                return null;
            }

            if(progress.TotalBytes.Value == 0) {
                return null;
            }

            return (double)progress.BytesWritten / (double)progress.TotalBytes.Value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cabinet.Core.Progress {
    public static class WriteProgressExtensions {
        public static double? ProgressPercentage(this IWriteProgress progress) {
            if(!progress.TotalBytes.HasValue) {
                return null;
            }

            return progress.BytesWritten / progress.TotalBytes.Value;
        }
    }
}

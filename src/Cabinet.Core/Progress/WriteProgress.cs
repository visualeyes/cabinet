using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Core {
    public class WriteProgress : IWriteProgress {
        public WriteProgress(string key, long bytesWritten, long? totalBytes) {
            Contract.NotNullOrEmpty(key, nameof(key));

            this.Key = key;
            this.BytesWritten = bytesWritten;
            this.TotalBytes = totalBytes;
        }

        public string Key { get; private set; }
        public long BytesWritten { get; private set; }
        public long? TotalBytes { get; private set; }
    }
}

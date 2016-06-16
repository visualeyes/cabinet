using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Core {
    public class WriteProgress : IWriteProgress {
        public long BytesWritten { get; set; }
        public long? TotalBytes { get; set; }
    }
}

using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Cabinet.Tests.Core {
    public class TestCabinetFileInfo : ICabinetFileInfo {
        private readonly Stream stream;

        public TestCabinetFileInfo(string key, bool exists, Stream stream) {
            this.Key = key;
            this.Exists = exists;
            this.stream = stream;
        }

        public string ProviderType { get; set; }
        public string Key { get; set; }
        public bool Exists { get; set; }

        public Stream GetFileReadStream() {
            return stream;
        }
    }
}

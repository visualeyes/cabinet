using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Cabinet.Tests.Core {
    public class TestCabinetFileInfo : ICabinetFileInfo {

        public TestCabinetFileInfo(string key, bool exists) {
            this.Key = key;
            this.Exists = exists;
        }

        public string ProviderType { get; set; }
        public string Key { get; set; }
        public bool Exists { get; set; }
    }
}

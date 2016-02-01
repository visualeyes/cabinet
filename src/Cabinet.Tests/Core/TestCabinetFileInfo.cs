using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Cabinet.Tests.Core {
    public class TestCabinetFileInfo : ICabinetItemInfo {

        public TestCabinetFileInfo(string key, bool exists, ItemType type) {
            this.Key = key;
            this.Exists = exists;
            this.Type = type;
        }

        public string ProviderType { get; set; }
        public string Key { get; set; }
        public bool Exists { get; set; }
        public ItemType Type { get; set; }
    }
}

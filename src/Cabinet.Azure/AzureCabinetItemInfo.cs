using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cabinet.Azure {
    public class AzureCabinetItemInfo : ICabinetItemInfo {

        public string ProviderType {
            get { return AzureCabinetConfig.ProviderType; }
        }

        public AzureCabinetItemInfo(string key, bool exists, ItemType type, DateTime? lastModifiedUtc) {
            if(String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            this.Key = key;
            this.Exists = exists;
            this.Type = type;
            this.LastModifiedUtc = lastModifiedUtc;
        }

        public string Key { get; private set; }
        public bool Exists { get; private set; }
        public ItemType Type { get; private set; }
        public DateTime? LastModifiedUtc { get; private set; }
    }
}

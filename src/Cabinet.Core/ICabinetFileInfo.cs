using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Core {
    public interface ICabinetItemInfo {
        string ProviderType { get; }
        ItemType Type { get; }

        bool Exists { get; }

        string Key { get; }

        long? Size { get; }
        DateTime? LastModifiedUtc { get; }
    }
}

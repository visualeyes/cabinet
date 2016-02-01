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
        string Key { get; }
        bool Exists { get; }
    }
}

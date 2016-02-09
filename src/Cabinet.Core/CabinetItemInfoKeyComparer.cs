using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Core {
    public class CabinetItemInfoKeyComparer : IEqualityComparer<ICabinetItemInfo> {

        public bool Equals(ICabinetItemInfo x, ICabinetItemInfo y) {
            return GetProviderKey(x) == GetProviderKey(y);
        }

        public int GetHashCode(ICabinetItemInfo obj) {
            return GetProviderHashCode(obj);
        }

        private string GetProviderKey(ICabinetItemInfo obj) {
            return String.Format("{0}-{1}", obj.ProviderType, obj.Key);
        }

        private int GetProviderHashCode(ICabinetItemInfo obj) {
            return GetProviderKey(obj).GetHashCode();
        }
    }
}

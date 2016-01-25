using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3 {
    public class S3CabinetFileInfoKeyComparer : IEqualityComparer<S3CabinetFileInfo>, IEqualityComparer<ICabinetFileInfo> {

        public bool Equals(ICabinetFileInfo x, ICabinetFileInfo y) {
            return GetProviderKey(x) == GetProviderKey(y);
        }

        public bool Equals(S3CabinetFileInfo x, S3CabinetFileInfo y) {
            return GetProviderKey(x) == GetProviderKey(y);
        }

        public int GetHashCode(ICabinetFileInfo obj) {
            return GetProviderHashCode(obj);
        }

        public int GetHashCode(S3CabinetFileInfo obj) {
            return GetProviderHashCode(obj);
        }

        private string GetProviderKey(ICabinetFileInfo obj) {
            return String.Format("{0}-{1}", obj.ProviderType, obj.Key);
        }

        private int GetProviderHashCode(ICabinetFileInfo obj) {
            return GetProviderKey(obj).GetHashCode();
        }
    }
}

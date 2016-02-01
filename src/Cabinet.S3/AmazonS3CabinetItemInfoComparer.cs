using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3 {
    public class S3CabinetFileInfoKeyComparer : IEqualityComparer<AmazonS3CabinetItemInfo>, IEqualityComparer<ICabinetItemInfo> {

        public bool Equals(ICabinetItemInfo x, ICabinetItemInfo y) {
            return GetProviderKey(x) == GetProviderKey(y);
        }

        public bool Equals(AmazonS3CabinetItemInfo x, AmazonS3CabinetItemInfo y) {
            return GetProviderKey(x) == GetProviderKey(y);
        }

        public int GetHashCode(ICabinetItemInfo obj) {
            return GetProviderHashCode(obj);
        }

        public int GetHashCode(AmazonS3CabinetItemInfo obj) {
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

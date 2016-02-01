using Amazon.S3.Model;
using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3 {
    public class AmazonS3CabinetItemInfo : ICabinetItemInfo {

        public string ProviderType {
            get { return AmazonS3StorageProvider.ProviderType; }
        }

        public AmazonS3CabinetItemInfo(string key, bool exists, ItemType type) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            this.Key = key;
            this.Exists = exists;
            this.Type = type;
        }

        public string Key { get; private set; }
        public bool Exists { get; private set; }
        public ItemType Type { get; private set; }

    }
}

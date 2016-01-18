using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Abstractions;

namespace Cabinet.FileSystem {
    public class CabinetFileInfo : ICabinetFileInfo {
        private readonly FileInfoBase fileInfo;

        public string ProviderType {
            get { return FileSystemStorageProvider.ProviderType; }
        }

        public CabinetFileInfo(string key, FileInfoBase fileInfo) {
            this.Key = key;
            this.fileInfo = fileInfo;
        }

        public string Key { get; private set; }

        public bool Exists {
            get { return this.fileInfo.Exists; }
        }

        public Stream GetFileReadStream() {
            if(!this.Exists) {
                throw new InvalidOperationException("Cannot get a read stream for a missing file");
            }

            return fileInfo.OpenRead();
        }
    }
}

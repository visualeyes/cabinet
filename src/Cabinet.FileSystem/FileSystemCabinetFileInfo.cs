using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Abstractions;

namespace Cabinet.FileSystem {
    internal class FileSystemCabinetFileInfo : ICabinetFileInfo {
        private readonly FileInfoBase fileInfo;

        public string ProviderType {
            get { return FileSystemStorageProvider.ProviderType; }
        }

        public FileSystemCabinetFileInfo(FileInfoBase fileInfo, string baseDirectory) {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
            if (String.IsNullOrWhiteSpace(baseDirectory)) throw new ArgumentNullException(nameof(baseDirectory));

            this.Key = GetFileKey(fileInfo, baseDirectory);
            this.fileInfo = fileInfo;
        }

        public string Key { get; private set; }

        public bool Exists {
            get { return this.fileInfo.Exists; }
        }

        public static string GetFileKey(FileInfoBase fileInfo, string baseDirectory) {
            return fileInfo.FullName.MakeRelativeTo(baseDirectory);
        }
    }
}

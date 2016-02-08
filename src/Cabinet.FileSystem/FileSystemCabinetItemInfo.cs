using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Abstractions;

namespace Cabinet.FileSystem {
    internal class FileSystemCabinetItemInfo : ICabinetItemInfo {

        public string ProviderType {
            get { return FileSystemCabinetConfig.ProviderType; }
        }

        public FileSystemCabinetItemInfo(FileSystemInfoBase itemInfo, string baseDirectory) {
            if (itemInfo == null) throw new ArgumentNullException(nameof(itemInfo));
            if (String.IsNullOrWhiteSpace(baseDirectory)) throw new ArgumentNullException(nameof(baseDirectory));

            this.Type = GetItemType(itemInfo);
            this.Key = GetItemKey(itemInfo, baseDirectory);
            this.Exists = itemInfo.Exists;
        }

        public ItemType Type { get; private set; }
        public string Key { get; private set; }
        public bool Exists { get; private set; }
        
        public static string GetItemKey(FileSystemInfoBase itemInfo, string baseDirectory) {
            return itemInfo.FullName.MakeRelativeTo(baseDirectory);
        }

        public static ItemType GetItemType(FileSystemInfoBase itemInfo) {
            if(itemInfo is FileInfoBase) {
                return ItemType.File;
            }

            if (itemInfo is DirectoryInfoBase) {
                return ItemType.Directory;
            }

            throw new NotImplementedException();
        }
    }
}

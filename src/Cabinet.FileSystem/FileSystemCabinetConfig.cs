using Cabinet.Core.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.FileSystem {
    public class FileSystemCabinetConfig : IStorageProviderConfig {
        public const string ProviderType = "FileSystem";

        public FileSystemCabinetConfig(string directory, bool createIfNotExists = false) {
            if (String.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException(nameof(directory));
            this.Directory = directory;
            this.CreateIfNotExists = createIfNotExists;
        }

        public string Delimiter => Path.DirectorySeparatorChar.ToString();
        public string Directory { get; }

        public bool CreateIfNotExists { get; set; }
    }
}

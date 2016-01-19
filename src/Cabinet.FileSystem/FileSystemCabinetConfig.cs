using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.FileSystem {
    public class FileSystemCabinetConfig : IFileCabinentConfig {
        public FileSystemCabinetConfig(string directory, bool createIfNotExists = false) {
            if (String.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException(nameof(directory));
            this.Directory = directory;
            this.CreateIfNotExists = createIfNotExists;
        }

        public string Directory { get; private set; }
        public bool CreateIfNotExists { get; set; }
    }
}

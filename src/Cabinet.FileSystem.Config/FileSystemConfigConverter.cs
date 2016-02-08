using Cabinet.Config;
using Cabinet.Core;
using Cabinet.Core.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.FileSystem.Config {
    internal class FileSystemConfigConverter : ICabinetProviderConfigConverter {
        private readonly IPathMapper pathMapper;

        public FileSystemConfigConverter(IPathMapper pathMapper) {
            Contract.NotNull(pathMapper, nameof(pathMapper));

            this.pathMapper = pathMapper;
        }

        public IStorageProviderConfig ToConfig(JToken config) {
            Contract.NotNull(config, nameof(config));

            string dir = config.Value<string>("dir");
            bool? createIfNotExists = config.Value<bool?>("createIfNotExists");

            if(!Path.IsPathRooted(dir)) {
                dir = pathMapper.MapPath(dir);
            }

            if(!createIfNotExists.HasValue) {
                createIfNotExists = false;
            }

            return new FileSystemCabinetConfig(dir, createIfNotExists.Value);
        }
    }
}

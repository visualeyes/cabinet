using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cabinet.Core.Providers;
using System.IO.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Cabinet.Config {
    public class FileCabinetProviderConfigStore : ICabinetProviderConfigStore {
        private const string TypeKey = "type";
        private const string ConfigKey = "config";

        private readonly string configFilePath;
        private readonly IFileCabinetConfigConverterFactory converterFactory;
        private readonly IFileSystem fs;

        public FileCabinetProviderConfigStore(string configFilePath, IFileCabinetConfigConverterFactory converterFactory)
            : this(configFilePath, converterFactory, new FileSystem()) {
        }

        public FileCabinetProviderConfigStore(string configFilePath, IFileCabinetConfigConverterFactory converterFactory, IFileSystem fs) {
            this.configFilePath = configFilePath;
            this.converterFactory = converterFactory;
            this.fs = fs;
        }

        public IStorageProviderConfig GetConfig(string name) {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            using (var configStream = fs.File.OpenRead(this.configFilePath)) {
                using (var streamReader = new StreamReader(configStream)) {
                    using (var jsonReader = new JsonTextReader(streamReader)) {

                        var config = JToken.Load(jsonReader, new JsonLoadSettings {
                            CommentHandling = CommentHandling.Ignore,
                            LineInfoHandling = LineInfoHandling.Ignore
                        });

                        var namedConfig = config[name];
                        if (namedConfig == null) {
                            return null;
                        }

                        string type = namedConfig.Value<string>(TypeKey);

                        if (String.IsNullOrWhiteSpace(type)) {
                            return null;
                        }

                        var converter = converterFactory.GetConverter(type);
                        var providerConfig = converter.ToConfig(namedConfig[ConfigKey]);

                        return providerConfig;
                    }
                }
            }
        }
    }
}

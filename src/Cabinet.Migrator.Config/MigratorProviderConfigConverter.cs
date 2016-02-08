using Cabinet.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cabinet.Core.Providers;
using Newtonsoft.Json.Linq;

namespace Cabinet.Migrator.Config {
    public class MigratorProviderConfigConverter : ICabinetProviderConfigConverter {
        private readonly ICabinetProviderConfigStore configStore;

        public MigratorProviderConfigConverter(ICabinetProviderConfigStore configStore) {
            this.configStore = configStore;
        }


        public IStorageProviderConfig ToConfig(JToken config) {
            string fromConfigName = config.Value<string>("from");
            string toConfigName = config.Value<string>("to");

            var fromConfig = configStore.GetConfig(fromConfigName);
            var toConfig = configStore.GetConfig(toConfigName);

            return new MigratorProviderConfig {
                FromConfig = fromConfig,
                ToConfig = toConfig
            };
        }
    }
}

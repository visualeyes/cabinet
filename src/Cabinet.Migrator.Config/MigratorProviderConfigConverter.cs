using Cabinet.Config;
using Cabinet.Core.Providers;
using Cabinet.Migrator.Migration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator.Config {
    public class MigratorProviderConfigConverter : ICabinetProviderConfigConverter {
        private readonly ICabinetProviderConfigStore configStore;

        public MigratorProviderConfigConverter(ICabinetProviderConfigStore configStore) {
            this.configStore = configStore;
        }


        public IStorageProviderConfig ToConfig(JToken config) {
            string fromConfigName = config.Value<string>("from");
            string toConfigName = config.Value<string>("to");

            var from = configStore.GetConfig(fromConfigName);
            var to = configStore.GetConfig(toConfigName);

            return new MigrationProviderConfig(from, to);
        }
    }
}

using Cabinet.Config;
using Cabinet.Core;
using Cabinet.Core.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Azure.Config {
    internal class AzureConfigConverter : ICabinetProviderConfigConverter {
    
        public IStorageProviderConfig ToConfig(JToken config) {
            Contract.NotNull(config, nameof(config));
            
            string connectionString = config.Value<string>("connectionString");
            string container = config.Value<string>("container");
            

            return new AzureCabinetConfig(connectionString, container);
        }
    }
}

using Cabinet.Core.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Config {
    public interface ICabinetProviderConfigConverter {
        IStorageProviderConfig ToConfig(JToken config);
    }
}

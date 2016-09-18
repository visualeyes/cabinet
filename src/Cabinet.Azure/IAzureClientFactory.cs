using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cabinet.Azure {
    public interface IAzureClientFactory {
        CloudBlobClient GetBlobClient(AzureCabinetConfig config);
    }
}

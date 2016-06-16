using System;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;

namespace Cabinet.Azure {
    internal class AzureClientFactory : IAzureClientFactory {

        public CloudBlobClient GetBlobClient(AzureCabinetConfig config) {
            return CloudStorageAccount
                .Parse(config.ConnectionString)
                .CreateCloudBlobClient();
        }
    }
}
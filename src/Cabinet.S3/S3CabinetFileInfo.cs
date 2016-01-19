using Amazon.S3.Model;
using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3 {
    public class S3CabinetFileInfo : ICabinetFileInfo {
        private readonly GetObjectResponse response;

        public S3CabinetFileInfo(GetObjectResponse response) {
            this.response = response;
        }

        public bool Exists {
            get { return response.HttpStatusCode == HttpStatusCode.OK; }
        }

        public string Key {
            get { return this.response.Key; }
        }

        public string ProviderType {
            get { return AmazonS3StorageProvider.ProviderType; }
        }

        public Stream GetFileReadStream() {
            return this.response.ResponseStream;
        }
    }
}

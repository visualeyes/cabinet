using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime;
using Cabinet.Core;

namespace Cabinet.S3.Config {
    internal class AWSCredentialsFactory : IAWSCredentialsFactory {
        public AWSCredentials GetBasicCredentials(string accessKey, string secretKey) {
            Contract.NotNullOrEmpty(accessKey, nameof(accessKey));
            Contract.NotNullOrEmpty(secretKey, nameof(secretKey));

            return new BasicAWSCredentials(accessKey, secretKey);
        }

        public AWSCredentials GetEnvironmentCredentials() {
            return new EnvironmentAWSCredentials();
        }

        public AWSCredentials GetEnvironmentVariableCredentials() {
            return new EnvironmentVariablesAWSCredentials();
        }

        public AWSCredentials GetInstanceProfileCredentials(string role) {
            Contract.NotNullOrEmpty(role, nameof(role));

            return new InstanceProfileAWSCredentials(role);
        }

        public AWSCredentials GetStoredProfileCredentials(string name) {
            return new StoredProfileAWSCredentials(name);
        }
    }
}

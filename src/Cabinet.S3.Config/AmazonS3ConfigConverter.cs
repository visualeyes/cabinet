using Amazon;
using Amazon.Runtime;
using Cabinet.Config;
using Cabinet.Core;
using Cabinet.Core.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3.Config {
    internal class AmazonS3ConfigConverter : ICabinetProviderConfigConverter {
        private readonly IAWSCredentialsFactory credentialsFactory;

        public AmazonS3ConfigConverter(IAWSCredentialsFactory credentialsFactory) {
            this.credentialsFactory = credentialsFactory;
        }

        public IStorageProviderConfig ToConfig(JToken config) {
            Contract.NotNull(config, nameof(config));
            
            string credentialsType = config.SelectToken("$.credentials.type").Value<string>();
            string regionString = config.Value<string>("region");
            string bucket = config.Value<string>("bucket");

            var credentials = GetCredentials(credentialsType, config["credentials"]);
            var region = RegionEndpoint.GetBySystemName(regionString);

            return new AmazonS3CabinetConfig(bucket, region, credentials);
        }

        private AWSCredentials GetCredentials(string credentialsType, JToken jToken) {
            Contract.NotNullOrEmpty(credentialsType, nameof(credentialsType));
            Contract.NotNull(jToken, nameof(jToken));

            switch(credentialsType.ToLower()) {
                case "basic":
                    string accessKey = jToken.Value<string>("access_key");
                    string secretKey = jToken.Value<string>("secret_key");
                    
                    return credentialsFactory.GetBasicCredentials(accessKey, secretKey);
                case "instance-profile":
                    string role = jToken.Value<string>("role");

                    return credentialsFactory.GetInstanceProfileCredentials(role);
                case "stored-profile":
                    string name = jToken.Value<string>("name");

                    return credentialsFactory.GetStoredProfileCredentials(name);
                case "environment":
                    return credentialsFactory.GetEnvironmentCredentials();
                case "environment-variables":
                    return credentialsFactory.GetEnvironmentVariableCredentials();
                default:
                    throw new NotImplementedException("Cannot get credentials with type: " + credentialsType);
            }
        }
    }
}

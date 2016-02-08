using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime;

namespace Cabinet.S3.Config {
    public interface IAWSCredentialsFactory {
        AWSCredentials GetBasicCredentials(string accessKey, string secretKey);
        AWSCredentials GetInstanceProfileCredentials(string role);
        AWSCredentials GetStoredProfileCredentials(string name);
        AWSCredentials GetEnvironmentCredentials();
        AWSCredentials GetEnvironmentVariableCredentials();
    }
}

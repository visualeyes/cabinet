using Amazon.Runtime;
using Cabinet.S3.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.S3.Config {
    public class AWSCredentialsFactoryFacts {
        private readonly AWSCredentialsFactory factory;

        public AWSCredentialsFactoryFacts() {
            this.factory = new AWSCredentialsFactory();
        }

        [Theory]
        [InlineData("accessKey", "secretKey")]
        public void Basic_Credentials(string accessKey, string secretKey) {
            var credentials = factory.GetBasicCredentials(accessKey, secretKey);
            var basicCredentials = credentials as BasicAWSCredentials;
            
            Assert.NotNull(basicCredentials);

            var immutableCredentials = basicCredentials.GetCredentials();
            Assert.Equal(accessKey, immutableCredentials.AccessKey);
            Assert.Equal(secretKey, immutableCredentials.SecretKey);
        }

        [Theory]
        [InlineData("accessKey", "")]
        [InlineData("accessKey", null)]
        [InlineData("", "secretKey")]
        [InlineData(null, "secretKey")]
        public void Basic_Credentials_Null_Throws(string accessKey, string secretKey) {
            Assert.Throws<ArgumentNullException>(() => factory.GetBasicCredentials(accessKey, secretKey));
        }
    }
}

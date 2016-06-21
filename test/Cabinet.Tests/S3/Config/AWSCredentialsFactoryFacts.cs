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

        [Fact]
        public void Environment_Credentials() {
            var credentials = factory.GetEnvironmentCredentials();
            var envAwsCredentials = credentials as EnvironmentAWSCredentials;

            Assert.NotNull(envAwsCredentials);
        }

        //[Fact] -- would require environment variables to be set
        //public void Environment_Variable_Credentials() {
        //    var credentials = factory.GetEnvironmentVariableCredentials();
            
        //    var envVarCrednetials = credentials as EnvironmentVariablesAWSCredentials;

        //    Assert.NotNull(envVarCrednetials);
        //}

        [Theory]
        [InlineData(""), InlineData("  "), InlineData(null)]
        public void Instance_Role_Credentials_Null_Throws(string role) {
            Assert.Throws<ArgumentNullException>(() => factory.GetInstanceProfileCredentials(role));
        }

        [Theory]
        [InlineData(""), InlineData("  "), InlineData(null)]
        public void Stored_Profile_Credentials_Null_Throws(string profile) {
            Assert.Throws<ArgumentNullException>(() => factory.GetStoredProfileCredentials(profile));
        }
    }
}

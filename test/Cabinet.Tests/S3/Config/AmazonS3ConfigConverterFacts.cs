using Amazon;
using Amazon.Runtime;
using Cabinet.S3;
using Cabinet.S3.Config;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.S3.Config {
    public class AmazonS3ConfigConverterFacts {
        private readonly Mock<IAWSCredentialsFactory> mockCredentialsFactory;
        private readonly AmazonS3ConfigConverter converter;

        public AmazonS3ConfigConverterFacts() {
            this.mockCredentialsFactory = new Mock<IAWSCredentialsFactory>();
            this.converter = new AmazonS3ConfigConverter(mockCredentialsFactory.Object);
        }

        [Theory]
        [MemberData("GetConfigStrings")]
        public void To_Config(
            string configStr,
            string expectedBucketName,RegionEndpoint expectedRegion, string expectedkeyPrefix, string expectedDelimiter,
            Expression<Action<IAWSCredentialsFactory>> verify
        ) {
            var configToken = JToken.Parse(configStr);

            var config = this.converter.ToConfig(configToken);

            Assert.NotNull(config);
            var s3Config = config as AmazonS3CabinetConfig;

            Assert.NotNull(s3Config);

            Assert.Equal(expectedBucketName, s3Config.BucketName);
            Assert.Equal(expectedRegion, s3Config.AmazonS3Config.RegionEndpoint);
            Assert.Equal(expectedkeyPrefix, s3Config.KeyPrefix);
            Assert.Equal(expectedDelimiter, s3Config.Delimiter);

            this.mockCredentialsFactory.Verify(verify, Times.Once);
        }


        public static object[][] GetConfigStrings() {
            string defaultKeyPrefix = null;
            string defaultDelimiter = AmazonS3CabinetConfig.DefaultDelimiter;

            Expression<Action<IAWSCredentialsFactory>> basicVerify = (f) => f.GetBasicCredentials(It.IsAny<string>(), It.IsAny<string>());
            Expression<Action<IAWSCredentialsFactory>> instanceProfileVerify = (f) => f.GetInstanceProfileCredentials(It.IsAny<string>());
            Expression<Action<IAWSCredentialsFactory>> storedProfileVerify = (f) => f.GetStoredProfileCredentials(It.IsAny<string>());
            Expression<Action<IAWSCredentialsFactory>> environmentVerify = (f) => f.GetEnvironmentCredentials();
            Expression<Action<IAWSCredentialsFactory>> environmentVariableVerify = (f) => f.GetEnvironmentVariableCredentials();
            
            return new [] { 
                //Types: basic, stored-profile, environment, environment-variable
                new object[] { @"{
                    ""credentials"": {
                        ""type"":  ""basic"",
                        ""access_key"": ""access"",
                        ""secret_key"": ""secret""
                    },
                    ""region"": ""ap-southeast-1"",
                    ""bucket"": ""test-bucket"",
                    ""keyPrefix"": ""production/a/""
                }",
                "test-bucket", RegionEndpoint.APSoutheast1, "production/a/", defaultDelimiter,
                basicVerify,
                
                },
                new object[] { @"{
                    ""credentials"": {
                        ""type"":  ""instance-profile"",
                        ""role"": ""some-role""
                    },
                    ""region"": ""ap-southeast-2"",
                    ""bucket"": ""test-bucket"",
                    ""delimiter"": "";""
                }",
                "test-bucket", RegionEndpoint.APSoutheast2, defaultKeyPrefix, ";",
                instanceProfileVerify
                },
                new object[] { @"{
                    ""credentials"": {
                        ""type"":  ""stored-profile"",
                        ""name"": ""some-name""
                    },
                    ""region"": ""ap-southeast-2"",
                    ""bucket"": ""test-bucket"",
                    ""keyPrefix"": ""production/b"",
                    ""delimiter"": "";""
                }",
                "test-bucket", RegionEndpoint.APSoutheast2, "production/b", ";",
                storedProfileVerify
                },
                new object[] { @"{
                    ""credentials"": {
                        ""type"":  ""environment""
                    },
                    ""region"": ""us-east-1"",
                    ""bucket"": ""test-bucket""
                }",
                "test-bucket", RegionEndpoint.USEast1, defaultKeyPrefix, defaultDelimiter,
                environmentVerify
                },
                new object[] { @"{
                    ""credentials"": {
                        ""type"":  ""environment-variables""
                    },
                    ""region"": ""us-west-2"",
                    ""bucket"": ""test-bucket""
                }",
                "test-bucket", RegionEndpoint.USWest2, defaultKeyPrefix, defaultDelimiter,
                environmentVariableVerify
                }
            };
        }
    }
}

using Cabinet.FileSystem;
using Cabinet.FileSystem.Config;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.FileSystem.Config {
    public class FileSystemConfigConverterFacts {
        private const string BaseWebDir = @"C:\test\web";

        private readonly FileSystemConfigConverter converter;
        private readonly Mock<IPathMapper> mockPathMapper;

        public FileSystemConfigConverterFacts() {
            this.mockPathMapper = new Mock<IPathMapper>();
            this.converter = new FileSystemConfigConverter(mockPathMapper.Object);

            this.mockPathMapper
                .Setup(p => p.MapPath(It.IsAny<string>()))
                .Returns<string>((val) => {
                    return BaseWebDir + val.Replace("~", "").Replace('/', '\\');
                });
        }

        [Theory]
        [MemberData("GetConfigStrings")]
        public void To_Config(string configStr, string expectedDir, bool expectedCreateIfExists) {
            var configToken = JToken.Parse(configStr);

            var config = this.converter.ToConfig(configToken);

            Assert.NotNull(config);
            var fileConfig = config as FileSystemCabinetConfig;

            Assert.NotNull(fileConfig);

            Assert.Equal(expectedDir, fileConfig.Directory);
            Assert.Equal(expectedCreateIfExists, fileConfig.CreateIfNotExists);
        }


        public static object[] GetConfigStrings() {
            return new object[] {
                new object[] { @"{
                    ""dir"": ""~/App_Data/Uploads"",
                    ""createIfNotExists"": true
                }",
                @"C:\test\web\App_Data\Uploads",
                true
                },
                new object[] { @"{
                    ""dir"": ""C:\\test\\data\\uploads"",
                    ""createIfNotExists"": false
                }",
                @"C:\test\data\uploads",
                false
                },
                new object[] { @"{
                    ""dir"": ""~/App_Data/Uploads""
                }",
                @"C:\test\web\App_Data\Uploads",
                false
                }
            };
        }
    }
}

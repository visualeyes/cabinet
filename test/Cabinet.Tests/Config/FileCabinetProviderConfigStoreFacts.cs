using Cabinet.Config;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Config {
    public class FileCabinetProviderConfigStoreFacts {
        private const string ConfigPath = @"c:\test\config";

        private readonly Mock<IFileCabinetConfigConverterFactory> mockConverterFactory;
        private readonly Mock<ICabinetProviderConfigConverter> mockConverter;
        private readonly MockFileSystem mockFs;

        public FileCabinetProviderConfigStoreFacts() {
            this.mockConverterFactory = new Mock<IFileCabinetConfigConverterFactory>();
            this.mockConverter = new Mock<ICabinetProviderConfigConverter>();
            this.mockFs = new MockFileSystem();
            this.mockConverterFactory.Setup(f => f.GetConverter(It.IsAny<string>())).Returns(mockConverter.Object);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public void Get_Config_NullEmpty_Name(string name) {
            SetupValidConfig();

            var store = GetConfigStore();

            Assert.Throws<ArgumentNullException>(() => store.GetConfig(name));
        }

        [Theory]
        [InlineData("ondisk", "FileSystem"), InlineData("amazon", "AmazonS3")]
        public void Get_Config(string name, string type) {
            SetupValidConfig();

            var store = GetConfigStore();

            var config = store.GetConfig(name);
            
            this.mockConverterFactory.Verify(f => f.GetConverter(type), Times.Once);
            this.mockConverter.Verify(c => c.ToConfig(It.IsAny<JToken>()), Times.Once);
        }

        [Theory]
        [InlineData("blah"), InlineData("missingType")]
        public void Get_Missing_Config(string name) {
            SetupValidConfig();

            var store = GetConfigStore();

            var config = store.GetConfig(name);

            Assert.Null(config);
        }

        private void SetupValidConfig() {
            string configJsonString = @"{
    ""ondisk"": {
        ""type"": ""FileSystem"",
        ""config"": {}
    },
    ""amazon"": {
        ""type"": ""AmazonS3"",
        ""config"": {}
    },
    ""missingType"": {
    }
}";
            this.mockFs.AddFile(ConfigPath, new MockFileData(configJsonString));
        }

        private FileCabinetProviderConfigStore GetConfigStore() {
            var store = new FileCabinetProviderConfigStore(ConfigPath, mockConverterFactory.Object, mockFs);
            return store;
        }
    }
}

using Cabinet.Config;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Config {
    public class FileCabinetConverterFactoryFacts {
        private readonly FileCabinetConfigConverterFactory converterFactory;

        public FileCabinetConverterFactoryFacts() {
            this.converterFactory = new FileCabinetConfigConverterFactory();
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData("  ")]
        public void NullEmpty_GetConverter_Throws(string providerType) {
            Assert.Throws<ArgumentNullException>(() => this.converterFactory.GetConverter(providerType));
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData("  ")]
        public void NullEmpty_RegisterProvider_Throws(string providerType) {
            var converter = new Mock<ICabinetProviderConfigConverter>();
            Assert.Throws<ArgumentNullException>(() => this.converterFactory.RegisterProvider(providerType, converter.Object));
        }

        [Fact]
        public void Null_Converter_Throws(string providerType) {
            ICabinetProviderConfigConverter converter = null;
            Assert.Throws<ArgumentNullException>(() => this.converterFactory.RegisterProvider("providerType", converter));
        }
    }
}

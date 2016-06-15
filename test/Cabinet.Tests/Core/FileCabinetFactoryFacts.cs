using Cabinet.Core;
using Cabinet.Core.Providers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Core {
    [Collection("FileCabinetFactory")]
    public class FileCabinetFactoryFacts {
        private readonly IFileCabinetFactory cabinetFactory;

        public FileCabinetFactoryFacts() {
            var factory = new FileCabinetFactory();
            factory.ClearCache();
            this.cabinetFactory = factory;
        }

        [Fact]
        public void Get_Provider_Throws_Null_Config() {
            TestProviderConfiguration config = null;
            Assert.Throws<ArgumentNullException>(() => cabinetFactory.GetCabinet(config));
        }

        [Fact]
        public void Get_Provider_Throws_If_Not_Registered() {
            var mockProviderConfig = new Mock<TestProviderConfiguration>();

            Assert.Throws<Exception>(() => cabinetFactory.GetCabinet(mockProviderConfig.Object));
        }

        [Fact]
        public void Get_Provider_Throws_If_Null_Provider_Returned() {
            Func<IStorageProvider<TestProviderConfiguration>> providerFactory = () => null;
            cabinetFactory.RegisterProvider(providerFactory);

            var mockProviderConfig = new Mock<TestProviderConfiguration>();

            Assert.Throws<Exception>(() => cabinetFactory.GetCabinet(mockProviderConfig.Object));
        }

        [Fact]
        public void Register_And_Get_Provider() {
            var mockProvider = new Mock<IStorageProvider<TestProviderConfiguration>>();

            cabinetFactory.RegisterProvider(mockProvider.Object);

            var providerConfig = new TestProviderConfiguration();
            var cabinet = cabinetFactory.GetCabinet(providerConfig);

            Assert.NotNull(cabinet);
        }

        [Fact]
        public void Register_Func_And_Get_Provider() {
            var mockProvider = new Mock<IStorageProvider<TestProviderConfiguration>>();
            Func<IStorageProvider<TestProviderConfiguration>> mockProviderFunc = () => mockProvider.Object;

            cabinetFactory.RegisterProvider(mockProviderFunc);

            var providerConfig = new TestProviderConfiguration();
            var cabinet = cabinetFactory.GetCabinet(providerConfig);

            Assert.NotNull(cabinet);
        }

        [Fact]
        public void Register_Provider_Throws_Null() {
            IStorageProvider<TestProviderConfiguration> provider = null;
            Assert.Throws<ArgumentNullException>(() => cabinetFactory.RegisterProvider(provider));
        }

        [Fact]
        public void Register_Provider_Factory_Throws_Null() {
            Func<IStorageProvider<TestProviderConfiguration>> providerFactory = null;
            Assert.Throws<ArgumentNullException>(() => cabinetFactory.RegisterProvider(providerFactory));
        }
    }
}

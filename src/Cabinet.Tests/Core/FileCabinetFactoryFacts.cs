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
            Assert.Throws<ArgumentNullException>(() => cabinetFactory.GetCabinet<ITestProviderConfiguration>(null));
        }

        [Fact]
        public void Get_Provider_Throws_If_Not_Registered() {
            var mockProviderConfig = new Mock<ITestProviderConfiguration>();

            Assert.Throws<ApplicationException>(() => cabinetFactory.GetCabinet(mockProviderConfig.Object));
        }

        [Fact]
        public void Get_Provider_Throws_If_Null_Provider_Returned() {
            Func<IStorageProvider<ITestProviderConfiguration>> providerFactory = () => null;
            cabinetFactory.RegisterProvider(providerFactory);

            var mockProviderConfig = new Mock<ITestProviderConfiguration>();

            Assert.Throws<ApplicationException>(() => cabinetFactory.GetCabinet(mockProviderConfig.Object));
        }

        [Fact]
        public void Register_And_Get_Provider() {
            var mockProvider = new Mock<IStorageProvider<ITestProviderConfiguration>>();

            cabinetFactory.RegisterProvider(mockProvider.Object);
            
            var mockProviderConfig = new Mock<ITestProviderConfiguration>();
            var cabinet = cabinetFactory.GetCabinet(mockProviderConfig.Object);

            Assert.NotNull(cabinet);
        }

        [Fact]
        public void Register_Func_And_Get_Provider() {
            var mockProvider = new Mock<IStorageProvider<ITestProviderConfiguration>>();
            Func<IStorageProvider<ITestProviderConfiguration>> mockProviderFunc = () => mockProvider.Object;

            cabinetFactory.RegisterProvider(mockProviderFunc);

            var mockProviderConfig = new Mock<ITestProviderConfiguration>();
            var cabinet = cabinetFactory.GetCabinet(mockProviderConfig.Object);

            Assert.NotNull(cabinet);
        }

        [Fact]
        public void Register_Provider_Throws_Null() {
            IStorageProvider<ITestProviderConfiguration> provider = null;
            Assert.Throws<ArgumentNullException>(() => cabinetFactory.RegisterProvider(provider));
        }

        [Fact]
        public void Register_Provider_Factory_Throws_Null() {
            Func<IStorageProvider<ITestProviderConfiguration>> providerFactory = null;
            Assert.Throws<ArgumentNullException>(() => cabinetFactory.RegisterProvider(providerFactory));
        }
    }
}

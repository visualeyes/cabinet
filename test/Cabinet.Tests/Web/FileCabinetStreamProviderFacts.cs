using Cabinet.Core;
using Cabinet.Web;
using Cabinet.Web.Validation;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Web {
    public class FileCabinetStreamProviderFacts {
        private readonly Mock<IFileCabinet> fileCabinet;
        private readonly Mock<IUploadValidator> fileValidator;
        private readonly Mock<IKeyProvider> keyProvider;
        private readonly string tempFileFolder;

        public FileCabinetStreamProviderFacts() {
            this.fileCabinet = new Mock<IFileCabinet>();
            this.fileValidator = new Mock<IUploadValidator>();
            this.keyProvider = new Mock<IKeyProvider>();
            this.tempFileFolder = "C:\\test\\upload";
        }

        [Fact]
        public void Null_Cabinet_Throws() {
            Assert.Throws<ArgumentNullException>(() => new FileCabinetStreamProvider(null, fileValidator.Object, keyProvider.Object, tempFileFolder));
        }

        [Fact]
        public void Null_Validator_Throws() {
            Assert.Throws<ArgumentNullException>(() => new FileCabinetStreamProvider(fileCabinet.Object, null, keyProvider.Object, tempFileFolder));
        }

        [Fact]
        public void Null_KeyProvider_Throws() {
            Assert.Throws<ArgumentNullException>(() => new FileCabinetStreamProvider(fileCabinet.Object, fileValidator.Object, null, tempFileFolder));
        }

        [InlineData]
        [InlineData(null), InlineData(""), InlineData("  ")]
        public void NullEmpty_FileFolder_Throws(string badTemp) {
            Assert.Throws<ArgumentNullException>(() => new FileCabinetStreamProvider(fileCabinet.Object, fileValidator.Object, keyProvider.Object, badTemp));
        }
        
        private FileCabinetStreamProvider GetStreamProvider() {
            return new FileCabinetStreamProvider(fileCabinet.Object, fileValidator.Object, keyProvider.Object, tempFileFolder);
        }
    }
}

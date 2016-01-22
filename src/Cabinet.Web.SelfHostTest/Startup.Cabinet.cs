using Cabinet.Core;
using Cabinet.FileSystem;
using Cabinet.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.SelfHostTest {
    public partial class Startup {
        public FileCabinetFactory ConfigureCabinet() {
            var cabinetFactory = new FileCabinetFactory();
            cabinetFactory
                .RegisterFileSystemProvider()
                .RegisterS3Provider();

            return cabinetFactory;
        }
    }
}

using Autofac;
using Autofac.Integration.WebApi;
using Cabinet.Core;
using Cabinet.FileSystem;
using Cabinet.S3;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Cabinet.Web.SelfHostTest {
    public partial class Startup {
		public ContainerBuilder ConfigureAutoFac(FileCabinetFactory cabinetFactory) {

            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterInstance<IPathMapper>(new PathMapper(AppDomain.CurrentDomain.SetupInformation.ApplicationBase));
            builder.RegisterType<UploadKeyProvider>().As<IKeyProvider>();

            builder.RegisterInstance<IFileCabinetFactory>(cabinetFactory);

            builder.Register((c) => {
                var mapper = c.Resolve<IPathMapper>();
                string uploadDir = mapper.MapPath("~/App_Data/Uploads");
                var fileConfig = new FileSystemCabinetConfig(uploadDir, true);
                return cabinetFactory.GetCabinet(fileConfig);
            });

            return builder;
        }
    }
}

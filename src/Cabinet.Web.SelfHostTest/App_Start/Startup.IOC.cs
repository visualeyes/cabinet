using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Autofac;
using Autofac.Integration.WebApi;
using Cabinet.Core;
using Cabinet.FileSystem;
using Cabinet.S3;
using Cabinet.Web.SelfHostTest.Framework;
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
        private const string BucketName = "test-bucket";

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

            //builder.Register((c) => {
            //    var s3Config = new AmazonS3CabinetConfig(BucketName, RegionEndpoint.APSoutheast2, new StoredProfileAWSCredentials());
            //    return cabinetFactory.GetCabinet(s3Config);
            //});

            return builder;
        }
    }
}

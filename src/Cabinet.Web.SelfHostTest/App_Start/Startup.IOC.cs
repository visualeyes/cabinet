using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Autofac;
using Autofac.Integration.WebApi;
using Cabinet.Config;
using Cabinet.Core;
using Cabinet.FileSystem;
using Cabinet.FileSystem.Config;
using Cabinet.S3;
using Cabinet.S3.Config;
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
        private const string ConfigFilePath = "~/cabinet-config.json";
        
		public ContainerBuilder ConfigureAutoFac() {
            var builder = new ContainerBuilder();

            // Web Stuff
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<System.IO.Abstractions.FileSystem>().As<System.IO.Abstractions.IFileSystem>();
            builder.RegisterType<UploadKeyProvider>().As<IKeyProvider>();

            // Cabinet Stuff
            var pathMapper = new PathMapper(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);

            var cabinetFactory = new FileCabinetFactory();
            var cabinetConfigFactory = new FileCabinetConfigConvertFactory();

            cabinetFactory
                .RegisterFileSystemProvider()
                .RegisterS3Provider();

            cabinetConfigFactory
                .RegisterFileSystemConfigConverter(pathMapper)
                .RegisterAmazonS3ConfigConverter();

            builder.RegisterInstance<IPathMapper>(pathMapper);
            builder.RegisterInstance<IFileCabinetFactory>(cabinetFactory);
            builder.RegisterInstance<IFileCabinetConfigConvertFactory>(cabinetConfigFactory);

            builder.Register<ICabinetProviderConfigStore>((c) => {
                var mapper = c.Resolve<IPathMapper>();
                string configPath = mapper.MapPath(ConfigFilePath);
                var converterFactory = c.Resolve<IFileCabinetConfigConvertFactory>();
                var fs = c.Resolve<System.IO.Abstractions.IFileSystem>();

                return new FileCabinetProviderConfigStore(configPath, converterFactory, fs);
            });

            // Register one cabinet for the whole app
            builder.Register<IFileCabinet>((c) => {
                var configStore = c.Resolve<ICabinetProviderConfigStore>();
                var config = configStore.GetConfig("amazon");
                return cabinetFactory.GetCabinet(config);
            });

            // Manual registration examples
            //builder.Register((c) => {
            //    var mapper = c.Resolve<IPathMapper>();
            //    string uploadDir = mapper.MapPath("~/App_Data/Uploads");
            //    var fileConfig = new FileSystemCabinetConfig(uploadDir, true);
            //    return cabinetFactory.GetCabinet(fileConfig);
            //});

            //builder.Register((c) => {
            //    var s3Config = new AmazonS3CabinetConfig("test-bucket", RegionEndpoint.APSoutheast2, new StoredProfileAWSCredentials());
            //    return cabinetFactory.GetCabinet(s3Config);
            //});

            return builder;
        }
    }
}

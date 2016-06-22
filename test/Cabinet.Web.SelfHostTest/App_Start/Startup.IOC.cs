using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Autofac;
using Autofac.Integration.WebApi;
using ByteSizeLib;
using Cabinet.Config;
using Cabinet.Core;
using Cabinet.FileSystem;
using Cabinet.FileSystem.Config;
using Cabinet.Migrator;
using Cabinet.Migrator.Config;
using Cabinet.S3;
using Cabinet.S3.Config;
using Cabinet.Web.Files;
using Cabinet.Web.SelfHostTest.Framework;
using Cabinet.Web.Validation;
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

            // Web Registrations
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<System.IO.Abstractions.FileSystem>().As<System.IO.Abstractions.IFileSystem>();

            // Cabinet.Web Registrations

            builder.RegisterType<FileTypeProvider>().As<IFileTypeProvider>();
            builder.RegisterType<UploadValidator>().As<IUploadValidator>();
            builder.RegisterType<UploadKeyProvider>().As<IKeyProvider>();
            builder.RegisterInstance<IValidationSettings>(new ValidationSettings {
                AllowedFileCategories = new FileTypeCategory[] { FileTypeCategory.Document, FileTypeCategory.Image },
                MaxSize = (long)ByteSize.FromMegaBytes(30).Bytes,
                MinSize = 1
            });

            // Cabinet Registrations
            var pathMapper = new PathMapper(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);

            string configPath = pathMapper.MapPath(ConfigFilePath);

            var cabinetFactory = new FileCabinetFactory();
            var cabinetConfigFactory = new FileCabinetConfigConverterFactory();
            var cabinetConfigStore = new FileCabinetProviderConfigStore(configPath, cabinetConfigFactory);

            cabinetFactory
                .RegisterFileSystemProvider()
                .RegisterS3Provider()
                .RegisterMigratorProvider();

            cabinetConfigFactory
                .RegisterFileSystemConfigConverter(pathMapper)
                .RegisterAmazonS3ConfigConverter()
                .RegisterMigratorConfigConverter(cabinetConfigStore);

            builder.RegisterInstance<IPathMapper>(pathMapper);
            builder.RegisterInstance<IFileCabinetFactory>(cabinetFactory);
            builder.RegisterInstance<IFileCabinetConfigConverterFactory>(cabinetConfigFactory);
            builder.RegisterInstance<ICabinetProviderConfigStore>(cabinetConfigStore);

            // Register one cabinet for the whole app
            builder.Register<IFileCabinet>((c) => {
                var configStore = c.Resolve<ICabinetProviderConfigStore>();
                var config = configStore.GetConfig("ondisk");
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

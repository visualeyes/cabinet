using Autofac.Integration.WebApi;
using Cabinet.Web.SelfHostTest.Framework;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Owin.Logging.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Cabinet.Web.SelfHostTest {
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            var cabinetFactory = ConfigureCabinet();
            var builder = ConfigureAutoFac(cabinetFactory);

            app.UseCommonLogging();
            //app.UseCommonLogging((log, type) => true, (log, type, message, exception) => {
            //    Console.WriteLine(message);
            //});

            app.Map("/api", apiApp => {
                var httpConfig = new HttpConfiguration();
                httpConfig.MapHttpAttributeRoutes();
                httpConfig.Filters.Add(new UnhandledExceptionFilter());
                //builder.RegisterWebApiFilterProvider(httpConfig);

                var container = builder.Build();
                httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container);

                apiApp.UseWebApi(httpConfig);
            });

            app.UseFileServer(new FileServerOptions() {
                RequestPath = PathString.Empty,
                FileSystem = new PhysicalFileSystem(@"./www"),
            });
        }
    }
}

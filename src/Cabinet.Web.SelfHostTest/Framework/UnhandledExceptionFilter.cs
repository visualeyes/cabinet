using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Cabinet.Web.SelfHostTest.Framework {
    public class UnhandledExceptionFilter : ExceptionFilterAttribute {
        private static ILog log = LogManager.GetLogger<UnhandledExceptionFilter>();

        public override void OnException(HttpActionExecutedContext context) {
            log.Error(context.Exception);
        }
    }
}

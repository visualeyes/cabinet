using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.SelfHostTest {
    public class PathMapper : IPathMapper {
        private readonly string appBase;

        public PathMapper(string appBase) {
            this.appBase = appBase;
        }

        public string MapPath(string path) {
            path = path
                    .TrimStart('~')
                    .TrimStart('/', '\\')
                    .Replace('/', '\\');

            return Path.Combine(appBase, path);
        }
    }
}

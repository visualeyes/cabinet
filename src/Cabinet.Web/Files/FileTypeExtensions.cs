using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.Files {
    public static class FileTypeExtensions {

        public static bool IsValidExtension(this IFileType type, string extension) {
            return type.Extensions.Contains(extension);
        }
    }
}

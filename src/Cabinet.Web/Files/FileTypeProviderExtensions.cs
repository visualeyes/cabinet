using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.Files {
    public static class FileTypeProviderExtensions {

        public static IFileType GetFileType(this IEnumerable<IFileType> types, string contentType) {
            var fileType = types.SingleOrDefault(t => t.ContentType == contentType || t.AlternativeContentTypes.Contains(contentType));

            return fileType;
        }
    }
}

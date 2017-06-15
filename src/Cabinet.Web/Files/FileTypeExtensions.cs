using System;
using System.Collections.Generic;
using System.Linq;

namespace Cabinet.Web.Files {
    public static class FileTypeExtensions {
        public static IFileType GetByExtension(this IEnumerable<IFileType> types, string extension) {
            return types.SingleOrDefault(t => t.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
        }

        public static IFileType GetByMimeType(this IEnumerable<IFileType> types, string mimeType) {
            return types.SingleOrDefault(t => t.MimeType.Equals(mimeType, StringComparison.OrdinalIgnoreCase) 
                || t.AlternativeMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase));
        }
    }
}

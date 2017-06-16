using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cabinet.Web.Files {
    public static class FileTypeExtensions {
        public static IFileType GetByFilePath(this IEnumerable<IFileType> types, string filePath) {
            if(filePath == null) throw new ArgumentNullException(nameof(filePath));

            return types.GetByExtension(Path.GetExtension(filePath));
        }

        public static IFileType GetByExtension(this IEnumerable<IFileType> types, string extension) {
            if(extension == null) throw new ArgumentNullException(nameof(extension));

            extension = extension.TrimStart('.');
            return types.SingleOrDefault(t => t.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
        }

        public static IFileType GetByMimeType(this IEnumerable<IFileType> types, string mimeType) {
            if(mimeType == null) throw new ArgumentNullException(nameof(mimeType));

            return types.SingleOrDefault(t => t.MimeType.Equals(mimeType, StringComparison.OrdinalIgnoreCase) 
                || t.AlternativeMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase));
        }
    }
}

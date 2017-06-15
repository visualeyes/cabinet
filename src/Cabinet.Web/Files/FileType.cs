using System.Collections.Generic;
using System.Linq;

namespace Cabinet.Web.Files {
    public class FileType : IFileType {

        public FileTypeCategory Category { get; }
        public string Name { get; }
        public string MimeType { get; }
        public IEnumerable<string> AlternativeMimeTypes { get; }
        public IEnumerable<string> Extensions { get; }

        public FileType(FileTypeCategory category, string name, string contentType, string extension, IEnumerable<string> altContentTypes = null)
            : this(category, name, contentType, new string[] { extension }, altContentTypes) {
        }

        public FileType(FileTypeCategory category, string name, string contentType, IEnumerable<string> extensions, IEnumerable<string> altContentTypes = null) {
            this.Category = category;
            this.Name = name;
            this.MimeType = contentType;
            this.Extensions = extensions;
            this.AlternativeMimeTypes = altContentTypes ?? Enumerable.Empty<string>();
        }
    }
}

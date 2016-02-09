using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.Files {
    public class FileType : IFileType {
        public FileType(FileTypeCategory category, string contentType, string extension, IEnumerable<string> altContentTypes = null)
            : this(category, contentType, new string[] { extension }, altContentTypes) {
        }

        public FileType(FileTypeCategory category, string contentType, IEnumerable<string> extensions, IEnumerable<string> altContentTypes = null) {
            this.Category = category;
            this.ContentType = contentType;
            this.Extensions = extensions;
            this.AlternativeContentTypes = altContentTypes ?? Enumerable.Empty<string>();
        }

        public FileTypeCategory Category { get; private set; }

        public string ContentType { get; private set; }
        public IEnumerable<string> AlternativeContentTypes { get; private set; }

        public IEnumerable<string> Extensions { get; private set; }
    }
}

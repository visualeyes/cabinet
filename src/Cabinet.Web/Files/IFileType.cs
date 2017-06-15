using System.Collections.Generic;

namespace Cabinet.Web.Files {
    public interface IFileType {
        FileTypeCategory Category { get; }
        string MimeType { get; }
        string Name { get; }
        IEnumerable<string> AlternativeMimeTypes { get; }
        IEnumerable<string> Extensions { get; }
    }
}
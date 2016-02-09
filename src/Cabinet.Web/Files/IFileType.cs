using System.Collections.Generic;

namespace Cabinet.Web.Files {
    public interface IFileType {
        FileTypeCategory Category { get; }
        string ContentType { get; }
        IEnumerable<string> AlternativeContentTypes { get; }
        IEnumerable<string> Extensions { get; }
    }
}
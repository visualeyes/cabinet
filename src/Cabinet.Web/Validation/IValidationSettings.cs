using System.Collections.Generic;
using Cabinet.Web.Files;

namespace Cabinet.Web.Validation {
    public interface IValidationSettings {
        IEnumerable<FileTypeCategory> AllowedFileCategories { get; }
        long MaxSize { get; }
        long MinSize { get; }
    }
}
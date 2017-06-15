using Cabinet.Web.Files;
using System.Collections.Generic;

namespace Cabinet.Web.Validation {
    public class ValidationSettings : IValidationSettings {
        public IEnumerable<FileTypeCategory> AllowedFileCategories { get; set; }
        public long MaxSize { get; set; }
        public long MinSize { get; set; }
    }
}

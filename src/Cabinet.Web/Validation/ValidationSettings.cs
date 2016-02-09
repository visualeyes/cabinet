using Cabinet.Web.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.Validation {
    public class ValidationSettings : IValidationSettings {
        public IEnumerable<FileTypeCategory> AllowedFileCategories { get; set; }
        public long MaxSize { get; set; }
        public long MinSize { get; set; }
    }
}

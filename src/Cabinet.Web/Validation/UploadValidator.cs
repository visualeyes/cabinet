using Cabinet.Web.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.Validation {
    public class UploadValidator : IUploadValidator {
        private readonly IFileTypeProvider typeProvider;

        public UploadValidator(IFileTypeProvider typeProvider) {
            this.typeProvider = typeProvider;
        }

        public bool IsFileTypeWhitelisted(string extension, string contentType, IEnumerable<FileTypeCategory> categories) {
            var types = typeProvider.GetFileTypes();
            var fileType = types.GetFileType(contentType);

            if(!fileType.IsValidExtension(extension)) {
                // Potential funny business
                return false;
            }

            return categories?.Contains(fileType.Category) ?? false;
        }
    }
}

using Cabinet.Core;
using Cabinet.Web.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.Validation {
    public class UploadValidator : IUploadValidator {
        private readonly IFileTypeProvider typeProvider;
        private readonly IValidationSettings validationSettings;

        public UploadValidator(IFileTypeProvider typeProvider, IValidationSettings validationSettings) {
            Contract.NotNull(typeProvider, nameof(typeProvider));
            Contract.NotNull(validationSettings, nameof(validationSettings));

            this.typeProvider = typeProvider;
            this.validationSettings = validationSettings;
        }

        public bool IsFileTooLarge(long fileSize) {
            return fileSize > this.validationSettings.MaxSize;
        }

        public bool IsFileTooSmall(long fileSize) {
            return fileSize < this.validationSettings.MinSize;
        }

        public bool IsFileTypeWhitelisted(string extension, string contentType) {
            if(String.IsNullOrWhiteSpace(extension)) return false;
            if(String.IsNullOrWhiteSpace(contentType)) return false;

            var types = typeProvider.GetFileTypes();
            var fileType = types.GetFileType(contentType);

            bool isValidExtension = fileType?.IsValidExtension(extension) ?? false;

            if (!isValidExtension) {
                // Potential funny business
                return false;
            }

            return validationSettings.AllowedFileCategories?.Contains(fileType.Category) ?? false;
        }
    }
}

using Cabinet.Core;
using Cabinet.Web.Files;
using System;
using System.Linq;

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

        public bool IsFileTypeWhitelisted(string extension) {
            if(String.IsNullOrWhiteSpace(extension)) return false;

            var fileType = typeProvider.GetFileTypes().GetByExtension(extension);
            if (fileType == null) {
                // Potential funny business
                return false;
            }

            return validationSettings.AllowedFileCategories?.Contains(fileType.Category) ?? false;
        }
    }
}

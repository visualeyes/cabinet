using Cabinet.Web.Files;
using Cabinet.Web.Validation;
using Xunit;

namespace Cabinet.Tests.Web.Validation {
    public class UploadValidatorFacts {

        [Theory]
        [InlineData(100, 1000, false)]
        [InlineData(100, 100, false)]
        [InlineData(1000, 100, true)]
        public void IsTooLarge(int size, int maxSize, bool tooLarge) {
            var validator = GetValidator(maxSize, 0);
            Assert.Equal(tooLarge, validator.IsFileTooLarge(size));
        }

        [Theory]
        [InlineData(100, 1000, true)]
        [InlineData(100, 100, false)]
        [InlineData(1000, 100, false)]
        public void IsTooSmall(int size, int minSize, bool tooLarge) {
            var validator = GetValidator(10000, minSize);
            Assert.Equal(tooLarge, validator.IsFileTooSmall(size));
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("  ", false)]
        [InlineData("txt", true)]
        [InlineData("text", true)]
        [InlineData("exe", false)]
        public void IsFileTypeWhitelisted(string extension, bool isValid) {
            var validator = GetValidator(10000, 0);
            Assert.Equal(isValid, validator.IsFileTypeWhitelisted(extension));
        }

        private UploadValidator GetValidator(int maxSize, int minSize) {
            var typeProvider = new FileTypeProvider();
            var settings = new ValidationSettings() {
                AllowedFileCategories = new FileTypeCategory[] {
                    FileTypeCategory.Document
                },
                MaxSize = maxSize,
                MinSize = minSize
            };

            return new UploadValidator(typeProvider, settings);
        }
    }
}

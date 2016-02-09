using Cabinet.Web.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.Validation {
    /// <summary>
    /// Provides a check to see if the extension / content type is allowed
    ///  -- https://www.owasp.org/index.php/Unrestricted_File_Upload
    ///  -- "Never accept a filename and its extension directly without having a white-list filter."
    /// </summary>
    public interface IUploadValidator {
        bool IsFileTypeWhitelisted(string extension, string contentType);
        bool IsFileTooLarge(long fileSize);
        bool IsFileTooSmall(long fileSize);
    }
}

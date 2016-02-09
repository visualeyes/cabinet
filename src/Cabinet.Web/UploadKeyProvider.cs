using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web {
    public class UploadKeyProvider : IKeyProvider {
        private const string UploadExt = ".upload";
        private const int MaxFileNameLength = 255;
        
        // Please consider security implication of selecting a key - https://www.owasp.org/index.php/Unrestricted_File_Upload

        public string GetKey(string fileName, string contentType) {
            string guid = Guid.NewGuid().ToString();

            var invalid = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars());

            foreach (char c in invalid) {
                fileName = fileName.Replace(c.ToString(), "_");
            }

            int maxUploadFileNameLength = MaxFileNameLength - UploadExt.Length;

            if (fileName.Length > maxUploadFileNameLength) {
                string ext = Path.GetExtension(fileName);
                fileName = fileName.Substring(0, maxUploadFileNameLength - ext.Length);
            }

            fileName += UploadExt;

            return Path.Combine(guid, fileName);
        }
    }
}

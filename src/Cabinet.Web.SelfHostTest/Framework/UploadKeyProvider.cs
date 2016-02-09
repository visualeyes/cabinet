using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.SelfHostTest.Framework {
    public class UploadKeyProvider : IKeyProvider {
        public string GetKey(string fileName, string contentType) {
            string guid = Guid.NewGuid().ToString();

            // Please consider security implication of selecting a key - https://www.owasp.org/index.php/Unrestricted_File_Upload

            string extensionLessFilename = Path.GetFileName(fileName);

            return Path.Combine(guid, extensionLessFilename);
        }
    }
}

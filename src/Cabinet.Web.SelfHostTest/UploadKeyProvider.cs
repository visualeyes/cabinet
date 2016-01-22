using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.SelfHostTest {
    public class UploadKeyProvider : IKeyProvider {
        public string GetKey(string fileName, string contentType) {
            string guid = Guid.NewGuid().ToString();
            string uploadedFileName = fileName.Trim('"').Trim('\\');
            string extensionLessFilename = Path.GetFileNameWithoutExtension(uploadedFileName);

            return Path.Combine(guid, extensionLessFilename);
        }
    }
}

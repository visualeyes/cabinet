using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web {
    /// <summary>
    /// Gets a key based on the file name and content type
    /// -- https://www.owasp.org/index.php/Unrestricted_File_Upload
    /// -- It is recommended to use an algorithm to determine the filenames. For instance, a filename can be a MD5 hash of the name of file plus the date of the day.
    /// </summary>
    public interface IKeyProvider {
        string GetKey(string fileName, string contentType);
    }
}

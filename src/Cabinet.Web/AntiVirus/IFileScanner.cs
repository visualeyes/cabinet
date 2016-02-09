using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.AntiVirus {
    public interface IFileScanner {
        Task ScanFileAsync(string filePath);
    }
}

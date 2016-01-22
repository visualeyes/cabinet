using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web {
    public interface IKeyProvider {
        string GetKey(string fileName, string contentType);
    }
}

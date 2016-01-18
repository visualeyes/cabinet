using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Core.Providers {
    public enum HandleExistingMethod {
        Throw = 0,
        Overwrite = 1,
        Skip = 2
    }
}

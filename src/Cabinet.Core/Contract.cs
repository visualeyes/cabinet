using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Core {
    public static class Contract {
        public static void NotNull(object obj, string paramName) {
            if (obj == null) throw new ArgumentNullException(paramName);
        }

        public static void NotNullOrEmpty(string str, string paramName) {
            if (String.IsNullOrWhiteSpace(str)) throw new ArgumentNullException(paramName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cabinet.Core.Exceptions {
    public class CabinetFileOpenException : Exception {

        public CabinetFileOpenException(string key, Exception e) 
            : base("Could not open read stream: " + key, e) {}
    }
}

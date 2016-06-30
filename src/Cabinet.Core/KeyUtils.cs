using Cabinet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cabinet.Core {
    public static class KeyUtils {

        public static string JoinKeys(string prefix, string key, string delimiter) {
            Contract.NotNullOrEmpty(delimiter, nameof(delimiter));

            if(String.IsNullOrWhiteSpace(prefix)) {
                return key ?? "";
            }
            
            if(String.IsNullOrWhiteSpace(key)) {
                return prefix;
            }

            if(!prefix.EndsWith(delimiter)) {
                prefix += delimiter;
            }

            if(key.StartsWith(delimiter)) {
                key = key.Remove(0, delimiter.Length);
            }

            return prefix + key;
        }
    }
}

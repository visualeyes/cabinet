using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Azure.Results {
    public class SaveResult : ISaveResult {
        private readonly string errorMsg;

        public SaveResult(string key, bool success = true) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            this.Key = key;
            this.Success = success;
        }

        public SaveResult(string key, Exception exception) {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            this.Key = key;
            this.Exception = exception;
            this.errorMsg = GetErrorMessage(exception);
            this.Success = false;
        }

        public string Key { get; private set; }

        public bool Success { get; private set; }

        public bool AlreadyExists { get; set; }

        public Exception Exception { get; private set; }

        public string GetErrorMessage() {
            return errorMsg;
        }

        private static string GetErrorMessage(Exception exception) {
            return exception?.ToString();
        }
    }
}

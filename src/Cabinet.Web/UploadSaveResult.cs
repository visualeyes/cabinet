using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web {
    public class UploadSaveResult : ISaveResult {
        private readonly string errorMessage;

        public UploadSaveResult(string key, string errorMessage) {
            this.Key = key;
            this.Success = false;
            this.errorMessage = errorMessage;
        }

        public string Key { get; private set; }
        public bool Success { get; private set; }
        public bool AlreadyExists { get; private set; }

        public Exception Exception { get; private set; }

        public string GetErrorMessage() {
            return this.errorMessage;
        }
    }
}

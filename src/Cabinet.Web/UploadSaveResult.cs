using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web {
    public class UploadSaveResult : ISaveResult {
        private readonly string errorMessage;

        public UploadSaveResult(string errorMessage) {
            this.Success = false;
            this.errorMessage = errorMessage;
        }

        public bool Success { get; private set; }
        public bool AlreadyExists { get; private set; }

        public Exception Exception { get; private set; }

        public string GetErrorMessage() {
            return this.errorMessage;
        }
    }
}

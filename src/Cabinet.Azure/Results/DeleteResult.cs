using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Azure.Results {
    public class DeleteResult : IDeleteResult {
        private readonly HttpStatusCode httpStatusCode;

        public DeleteResult() {
        }

        public DeleteResult(Exception exception) {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            this.Exception = exception;
        }
        
        public Exception Exception { get; private set; }

        public bool Success {
            get { return this.Exception == null && this.httpStatusCode == HttpStatusCode.NoContent; }
        }

        public string GetErrorMessage() {
            if (this.Exception != null) {
                return this.Exception.ToString();
            }
            return null;
        }
    }
}

using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3.Results {
    public class SaveResult : ISaveResult {
        private readonly string errorMsg;

        public SaveResult(bool success = true) {
            this.Success = success;
        }

        public SaveResult(HttpStatusCode code) {
            this.Success = code == HttpStatusCode.OK;
            this.errorMsg = GetErrorMessage(code);
        }

        public SaveResult(Exception exception) {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            this.Exception = exception;
            this.errorMsg = GetErrorMessage(exception);
            this.Success = false;
        }

        public bool AlreadyExists { get; set; }

        public Exception Exception { get; private set; }

        public bool Success { get; private set; }

        public string GetErrorMessage() {
            return errorMsg;
        }

        public static string GetErrorMessage(Exception exception) {
            return exception?.ToString();
        }

        public static string GetErrorMessage(HttpStatusCode httpStatusCode) {
            switch (httpStatusCode) {
                case HttpStatusCode.NotFound:
                    return "File does not exist";
                case HttpStatusCode.Unauthorized:
                    return "Access to the bucket is denied";
                default:
                    return null;
            }
        }
    }
}

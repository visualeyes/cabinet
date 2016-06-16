using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Azure.Results {
    public class MoveResult : IMoveResult {
        private readonly string errorMsg;

        private MoveResult(string sourceKey, string destKey) {
            if (String.IsNullOrWhiteSpace(sourceKey)) throw new ArgumentNullException(nameof(sourceKey));
            if (String.IsNullOrWhiteSpace(destKey)) throw new ArgumentNullException(nameof(destKey));
            this.SourceKey = sourceKey;
            this.DestKey = destKey;
        }

        public MoveResult(string sourceKey, string destKey, bool success = true)
            : this(sourceKey, destKey) {
            this.SourceKey = sourceKey;
            this.DestKey = destKey;
            this.Success = success;
        }
        
        public MoveResult(string sourceKey, string destKey, Exception exception, string errorMsg = null)
            : this(sourceKey, destKey) {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            this.Exception = exception;
            this.errorMsg = errorMsg ?? GetErrorMessage(exception);
            this.Success = false;
        }

        public string SourceKey { get; private set; }
        public string DestKey { get; private set; }

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

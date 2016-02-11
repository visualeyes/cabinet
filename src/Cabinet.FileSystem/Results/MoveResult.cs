using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.FileSystem.Results {
    internal class MoveResult : IMoveResult {
        private readonly string errorMsg;

        private MoveResult(string sourceKey, string destKey, string errorMsg) {
            if (String.IsNullOrWhiteSpace(sourceKey)) throw new ArgumentNullException(nameof(sourceKey));
            if (String.IsNullOrWhiteSpace(destKey)) throw new ArgumentNullException(nameof(destKey));
            this.SourceKey = sourceKey;
            this.DestKey = destKey;

            this.errorMsg = errorMsg;
        }

        public MoveResult(string sourceKey, string destKey, bool success = true, string errorMsg = null) 
            : this(sourceKey, destKey, errorMsg) {
            this.Success = success;
        }

        public MoveResult(string sourceKey, string destKey, Exception e, string errorMsg = null)
            : this(sourceKey, destKey, errorMsg ?? GetMoveFileErrorMessage(e)) {
            if (e == null) throw new ArgumentNullException(nameof(e));
            this.SourceKey = sourceKey;
            this.DestKey = destKey;
            this.Exception = e;
            this.Success = false;
        }

        public string SourceKey { get; private set; }
        public string DestKey { get; private set; }
        public bool Success { get; private set; }

        public Exception Exception { get; private set; }

        public bool AlreadyExists { get; set; }

        public string GetErrorMessage() {
            return errorMsg;
        }

        internal static string GetMoveFileErrorMessage(Exception e) {
            string errorMsg = null;
            
            if (e is UnauthorizedAccessException) {
                errorMsg = "Could not move the file";
            } else if (e is PathTooLongException) {
                errorMsg = "The path is too long. The path must be less than 248 characters and file name less than 260 characters.";
            } else if (e is DirectoryNotFoundException) {
                errorMsg = "The source or destination directory could not be found";
            } else if (e is NotSupportedException) {
                errorMsg = "The source or destination name is not valid";
            } else if (e is IOException) {
                errorMsg = "Destination file already exists";
            }

            return errorMsg;
        }
    }
}

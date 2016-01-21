using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.FileSystem.Results {
    internal class DeleteResult : IDeleteResult {

        public DeleteResult(bool success = true) {
            this.Success = success;
        }

        public DeleteResult(Exception e) {
            if (e == null) throw new ArgumentNullException(nameof(e));
            this.Exception = e;
            this.Success = false;
        }

        public bool Success { get; private set; }

        public Exception Exception { get; private set; }

        public bool AlreadyDeleted { get; set; }

        public string GetErrorMessage() {
            string errorMsg = null;

            if (this.Exception is DirectoryNotFoundException) {
                errorMsg = "Could not find the file";
            } else if (this.Exception is PathTooLongException ) {
                errorMsg = "The path is too long. The path must be less than 248 characters and file name less than 260 characters.";
            } else if (this.Exception is UnauthorizedAccessException) {
                errorMsg = "Could not delete the file";
            } else if (this.Exception is NotSupportedException) {
                errorMsg = "The path is not valid";
            } else if (this.Exception is IOException) {
                errorMsg = "The file is in use, is read-only or is a directory";
            }

            return errorMsg;
        }

    }
}

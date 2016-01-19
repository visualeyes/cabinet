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

        public MoveResult(bool success = true) {
            this.Success = success;
        }

        public MoveResult(Exception e, string errorMsg = null) {
            this.Exception = e;
            this.errorMsg = errorMsg ?? GetMoveFileErrorMessage(e);
            this.Success = false;
        }

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

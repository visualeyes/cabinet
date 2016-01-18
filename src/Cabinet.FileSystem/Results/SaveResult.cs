using Cabinet.Core.Providers.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.FileSystem.Results {
    public class SaveResult : ISaveResult {
        private readonly string errorMsg;

        public SaveResult(bool success = true) {
            this.Success = success;
        }

        public SaveResult(Exception e, string errorMsg = null) {
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

        internal static string GetMoveFileErrorMessage(Exception Exception) {
            string errorMsg = null;

            if (Exception is UnauthorizedAccessException) {
                errorMsg = "Could not save the file";
            } else if (Exception is PathTooLongException) {
                errorMsg = "The path is too long. The path must be less than 248 characters and file name less than 260 characters.";
            } else if (Exception is DirectoryNotFoundException) {
                errorMsg = "The destination directory could not be found";
            } else if (Exception is NotSupportedException) {
                errorMsg = "The destination name is not valid";
            } else if (Exception is IOException) {
                errorMsg = "Destination file already exists";
            }

            return errorMsg;
        }
    }
}

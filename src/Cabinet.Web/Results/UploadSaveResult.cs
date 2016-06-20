using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cabinet.Core;

namespace Cabinet.Web.Results {
    public class UploadSaveResult : ISaveResult {
        private readonly string errorMessage;

        private UploadSaveResult(string uploadFileName, string uploadFileMediaType) {
            Contract.NotNullOrEmpty(uploadFileName, nameof(uploadFileName));
            Contract.NotNullOrEmpty(uploadFileMediaType, nameof(uploadFileMediaType));

            this.UploadFileName = uploadFileName;
            this.UploadFileMediaType = uploadFileMediaType;
        }

        public UploadSaveResult(string uploadFileName, string uploadFileMediaType, string errorMessage) 
            : this(uploadFileName, uploadFileMediaType) {
            Contract.NotNullOrEmpty(errorMessage, nameof(errorMessage));
            
            this.Success = false;
            this.errorMessage = errorMessage;
        }

        public UploadSaveResult(string uploadFileName, string uploadFileMediaType, ISaveResult result)
            : this(uploadFileName, uploadFileMediaType) {
            Contract.NotNull(result, nameof(result));
            
            this.Success = result.Success;
            this.Exception = result.Exception;
            this.errorMessage = result.GetErrorMessage();
            this.Key = result.Key;
            this.AlreadyExists = result.AlreadyExists;
        }

        public string Key { get; }
        public bool AlreadyExists { get; }

        public bool Success { get; }
        public Exception Exception { get; }

        public string UploadFileName { get; }
        public string UploadFileMediaType { get; }

        public string GetErrorMessage() {
            return this.errorMessage;
        }
    }
}

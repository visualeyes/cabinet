using Cabinet.Core;
using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator.Results {
    public class DeleteResult : IDeleteResult {
        private readonly IDeleteResult fromDeleteResult;
        private readonly IDeleteResult toDeleteResult;

        public DeleteResult(IDeleteResult fromDeleteResult, IDeleteResult toDeleteResult) {
            Contract.NotNull(fromDeleteResult, nameof(fromDeleteResult));
            Contract.NotNull(toDeleteResult, nameof(toDeleteResult));

            this.fromDeleteResult = fromDeleteResult;
            this.toDeleteResult = toDeleteResult;
        }

        public Exception Exception {
            get {
                bool fromHasException = fromDeleteResult.Exception != null;
                bool toHasException = toDeleteResult.Exception != null;

                if(fromHasException && toHasException) {
                    return new AggregateException(fromDeleteResult.Exception, toDeleteResult.Exception);
                }

                if(fromHasException) return fromDeleteResult.Exception;
                if(toHasException) return toDeleteResult.Exception;

                return null;
            }
        }

        public bool Success {
            get { return fromDeleteResult.Success && toDeleteResult.Success; }
        }

        public string GetErrorMessage() {
            string fromError = this.fromDeleteResult.GetErrorMessage();
            string toError = this.toDeleteResult.GetErrorMessage();

            if(String.IsNullOrWhiteSpace(fromError) && String.IsNullOrWhiteSpace(toError)) {
                return null;
            }

            var sb = new StringBuilder("Migration delete error.");
            sb.AppendLine("From Cabinet:");
            sb.AppendLine(fromError);
            sb.AppendLine("To Cabinet:");
            sb.AppendLine(toError);

            return sb.ToString();
        }
    }
}

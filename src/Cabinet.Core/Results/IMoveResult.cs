using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Core.Results {
    public interface IMoveResult : IFileOperationResult {
        string SourceKey { get; }
        string DestKey { get; }
        bool AlreadyExists { get; }
    }
}
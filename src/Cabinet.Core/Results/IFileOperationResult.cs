using System;

namespace Cabinet.Core.Results {
    public interface IFileOperationResult {
        Exception Exception { get; }
        bool Success { get; }

        string GetErrorMessage();
    }
}
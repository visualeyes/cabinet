using System;

namespace Cabinet.Core.Providers.Results {
    public interface IFileOperationResult {
        Exception Exception { get; }
        bool Success { get; }

        string GetErrorMessage();
    }
}
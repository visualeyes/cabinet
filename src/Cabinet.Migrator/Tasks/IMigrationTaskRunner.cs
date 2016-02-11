using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cabinet.Migrator {
    public interface IMigrationTaskRunner {
        Task RunTasks<I, T>(Func<I, Task<T>> task, IEnumerable<I> inputs, CancellationToken cancellationToken);
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cabinet.Migrator {
    public interface IMigrationTaskRunner {
        Task RunTasks<I>(Func<I, Task> task, IEnumerable<I> inputs, CancellationToken cancellationToken);
    }
}
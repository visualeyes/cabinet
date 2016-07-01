using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Cabinet.Migrator {
    public class MigrationTaskRunner : IMigrationTaskRunner {
        private const int BatchSize = 20;

        public async Task RunTasks<I>(Func<I, Task> task, IEnumerable<I> inputs, CancellationToken cancellationToken) {
            var taskBlock = new ActionBlock<I>(task, new ExecutionDataflowBlockOptions {
                BoundedCapacity = BatchSize,
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 5
            });

            foreach (var key in inputs) {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.WhenAny(taskBlock.SendAsync(key, cancellationToken), taskBlock.Completion);
            }

            taskBlock.Complete();
            
            await taskBlock.Completion;
        }
    }
}

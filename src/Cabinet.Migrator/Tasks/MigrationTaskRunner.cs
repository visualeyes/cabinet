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

        public async Task RunTasks<I, T>(Func<I, Task<T>> task, IEnumerable<I> inputs, CancellationToken cancellationToken) {
            var buffer = new BufferBlock<I>(new DataflowBlockOptions {
                BoundedCapacity = BatchSize,
                CancellationToken = cancellationToken
            });

            var taskBlock = new TransformBlock<I, T>(task, new ExecutionDataflowBlockOptions {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 5
            });

            buffer.LinkTo(taskBlock, new DataflowLinkOptions { PropagateCompletion = true });

            foreach (var key in inputs) {
                await Task.WhenAny(buffer.SendAsync(key, cancellationToken), taskBlock.Completion);
            }

            buffer.Complete();

            await taskBlock.Completion;
        }
    }
}

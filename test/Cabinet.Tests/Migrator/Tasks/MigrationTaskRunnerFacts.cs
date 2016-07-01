using Cabinet.Migrator;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cabinet.Tests.Migrator.Tasks {
    public class MigrationTaskRunnerFacts {
        public interface ITestTask {
            Task OperationAsync(int i);
        }

        [Theory]
        [InlineData(0), InlineData(1), InlineData(10), InlineData(100)]
        public async Task Run_Tasks(int numItems) {
            var mockTask = new Mock<ITestTask>();
            mockTask
                .Setup(t => t.OperationAsync(It.IsInRange(0, numItems, Range.Inclusive)))
                .Returns(Task.FromResult(0));

            var inputs = Enumerable.Range(0, numItems);

            var runner = new MigrationTaskRunner();

            await runner.RunTasks(async (i) => {
                await mockTask.Object.OperationAsync(i);
            }, inputs, CancellationToken.None);

            mockTask.Verify(t => t.OperationAsync(It.IsInRange(0, numItems, Range.Inclusive)), Times.Exactly(numItems));
        }
    }
}

using Cabinet.Core.Progress;
using ManyConsole;
using System;
using System.IO;

namespace Cabinet.ConsoleTest {
    public class DownloadCommand : ConsoleCommand {
        private string configName;
        private string key;
        private string filePath;

        public DownloadCommand() {
            IsCommand("download", "Gets a file from the cabinet");

            HasRequiredOption("cabinet=|c=", "Cabinet to download the file from", c => configName = c);
            HasRequiredOption("key=|k=", "Key to of the file to download", k => key = k);
            HasRequiredOption("file-path=|f=", "File Path to save file to", f => filePath = f);
        }

        public override int Run(string[] remainingArguments) {
            var config = Program.CabinetConfigStore.GetConfig(configName);
            var cabinet = Program.CabinetFactory.GetCabinet(config);

            Console.WriteLine($"Starting download {key}...");

            Nito.AsyncEx.AsyncContext.Run(async () => {
                using(var writeStream = File.OpenWrite(filePath)) {
                    using(var readStream = await cabinet.OpenReadStreamAsync(key)) {
                        long? length = readStream.TryGetStreamLength();
                        var progressStream = new ProgressStream(key, writeStream, length, new ConsoleProgress());
                        await readStream.CopyToAsync(progressStream);
                    }
                }
            });

            Console.WriteLine();
            Console.WriteLine($"Completed downloading {key} to {filePath}");

            return 0;
        }
    }
}

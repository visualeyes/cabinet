using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.ConsoleTest {
    public class SaveCommand : ConsoleCommand {
        private string configName;
        private string filePath;
        private string key;

        private HandleExistingMethod handleExisting = HandleExistingMethod.Throw;

        public SaveCommand() {
            IsCommand("save", "Saves a file into the cabinet");

            HasRequiredOption("cabinet=|c=", "Cabinet to save file to", c => configName = c);
            HasRequiredOption("file-path=|f=", "Path of the File to save", f => filePath = f);
            HasRequiredOption("key=|k=", "Key to save file to", k => key = k);

            HasOption("overwrite|o", "Overwrite an existing file in the cabinet", o => handleExisting = HandleExistingMethod.Overwrite);
            HasOption("skip|s", "Skip if there is an existing file in the cabinet", o => handleExisting = HandleExistingMethod.Skip);
        }

        public override int Run(string[] remainingArguments) {
            var config = Program.CabinetConfigStore.GetConfig(configName);
            var cabinet = Program.CabinetFactory.GetCabinet(config);

            var result = Nito.AsyncEx.AsyncContext.Run(async () => {
                return await cabinet.SaveFileAsync(key, filePath, handleExisting, new ConsoleProgress());
            });

            Console.WriteLine();

            if (result.Success) {
                Console.WriteLine("Uploaded {0} to {1}", filePath, key);
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to upload {0} to {1}", filePath, key);
                Console.WriteLine(result.GetErrorMessage());
                Console.ForegroundColor = ConsoleColor.White;
            }

            return result.Success ? 0 : -1;
        }
    }
}

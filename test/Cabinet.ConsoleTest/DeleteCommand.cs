using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.ConsoleTest {
    public class DeleteCommand : ConsoleCommand {
        private string configName;
        private string key;
        
        public DeleteCommand() {
            IsCommand("delete", "Deletes a file from the cabinet");

            HasRequiredOption("cabinet=|c=", "Cabinet to save file to", c => configName = c);
            HasRequiredOption("key=|k=", "Key to save file to", k => key = k);
        }

        public override int Run(string[] remainingArguments) {
            var config = Program.CabinetConfigStore.GetConfig(configName);
            var cabinet = Program.CabinetFactory.GetCabinet(config);

            var result = Nito.AsyncEx.AsyncContext.Run(async () => {
                return await cabinet.DeleteFileAsync(key);
            });

            Console.WriteLine();

            if (result.Success) {
                Console.WriteLine("Deleted key: {0}", key);
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to delete key: {0}", key);
                Console.WriteLine(result.GetErrorMessage());
                Console.ForegroundColor = ConsoleColor.White;
            }

            return result.Success ? 0 : -1;
        }
    }
}

using ByteSizeLib;
using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.ConsoleTest {
    public class GetCommand : ConsoleCommand {
        private string configName;
        private string key;

        private HandleExistingMethod handleExisting = HandleExistingMethod.Throw;

        public GetCommand() {
            IsCommand("get", "Gets a file from the cabinet");

            HasRequiredOption("cabinet=|c=", "Cabinet to get the file from", c => configName = c);
            HasRequiredOption("key=|k=", "Key to of the file to get", k => key = k);
        }

        public override int Run(string[] remainingArguments) {
            var config = Program.CabinetConfigStore.GetConfig(configName);
            var cabinet = Program.CabinetFactory.GetCabinet(config);

            var result = Nito.AsyncEx.AsyncContext.Run(async () => {
                return await cabinet.GetItemAsync(key);
            });

            Console.WriteLine();
            
            if (result.Exists) {
                Console.WriteLine($"Got the file {result.Key}: ");
                Console.WriteLine($"  Provider: {result.ProviderType}");
                if(result.Size.HasValue) {
                    Console.WriteLine($"  Size: {ByteSize.FromBytes(result.Size.Value)}");
                } else {
                    Console.WriteLine($"  Size: unknown");
                }
                Console.WriteLine($"  Last Modified: {result.LastModifiedUtc}");                
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{key} does not exist");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return result.Exists ? 0 : -1;
        }
    }
}

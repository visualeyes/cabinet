using Cabinet.Config;
using Cabinet.Core;
using Cabinet.FileSystem;
using Cabinet.FileSystem.Config;
using Cabinet.S3;
using Cabinet.S3.Config;
using Cabinet.Web.ConsoleTest;
using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.ConsoleTest {
    class Program {
        private const string ConfigFilePath = "~/cabinet-config.json";

        public static readonly FileCabinetFactory CabinetFactory;
        public static readonly FileCabinetConfigConvertFactory CabinetConfigFactory;
        public static readonly FileCabinetProviderConfigStore CabinetConfigStore;

        static Program() {
            var pathMapper = new PathMapper(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);

            string configPath = pathMapper.MapPath(ConfigFilePath);

            CabinetFactory = new FileCabinetFactory();
            CabinetConfigFactory = new FileCabinetConfigConvertFactory();
            CabinetConfigStore = new FileCabinetProviderConfigStore(configPath, CabinetConfigFactory);

            CabinetFactory
                .RegisterFileSystemProvider()
                .RegisterS3Provider();

            CabinetConfigFactory
                .RegisterFileSystemConfigConverter(pathMapper)
                .RegisterAmazonS3ConfigConverter();

        }

        static int Main(string[] args) {
            var commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));

            if (args.Length == 0) {
                Console.Write("Command to execute: ");
                string subcommand = Console.ReadLine();
                args = new string[] { subcommand };
            }

            int result = ConsoleCommandDispatcher.DispatchCommand(
                commands: commands,
                arguments: args,
                consoleOut: Console.Out,
                skipExeInExpectedUsage: true
            );

            return result;
        }
    }
}

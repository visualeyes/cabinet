using Cabinet.Core;
using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator {
    //TODO
    public class CabinetMigrator {

        public async Task Migrate(IFileCabinet sourceCabinet, IFileCabinet destCabinet, bool deleteSource) {
            throw new NotImplementedException();

            var allKeys = await sourceCabinet.ListKeysAsync(recursive: true);

            // TODO: tpl data flow to create tasks to migrate files

        }

        //TODO: return results
        public async Task Migrate(IFileCabinet sourceCabinet, string sourceKey, IFileCabinet destCabinet, string destKey, bool deleteSource, HandleExistingMethod handleExisting = HandleExistingMethod.Skip) {
            var file = await sourceCabinet.GetFileAsync(sourceKey);
            if(!file.Exists) {
                return;
            }

            ISaveResult saveResult;
            using (var stream = await sourceCabinet.OpenFileReadStream(file)) {
                saveResult = await destCabinet.SaveFileAsync(file.Key, stream, handleExisting);
            }

            if(!saveResult.Success) {
                return;
            }

            if (deleteSource) {
                var deleteResult = await sourceCabinet.DeleteFileAsync(sourceKey);

            }
        }
    }
}

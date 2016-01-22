using Cabinet.Core;
using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web {
    public class FileCabinetStreamProvider : MultipartFileStreamProvider {
        private readonly IFileCabinet fileCabinet;
        private readonly IKeyProvider keyProvider;

        public FileCabinetStreamProvider(IFileCabinet fileCabinet, IKeyProvider keyProvider, string tempFileFolder) 
            : base(tempFileFolder) {
            this.fileCabinet = fileCabinet;
            this.keyProvider = keyProvider;
        }

        public async Task<ISaveResult[]> SaveInCabinet(HandleExistingMethod handleExisting) {

            var saveTasks = this.FileData.Select(async (fd) => {
                string key = this.keyProvider.GetKey(fd.Headers.ContentDisposition.FileName, fd.Headers.ContentType?.MediaType);

                var fileInfo = new FileInfo(fd.LocalFileName);

                if (!fileInfo.Exists) {
                    return new UploadSaveResult("Could not find uploaded file.");
                }

                using (var fileStream = fileInfo.OpenRead()) {
                    return await this.fileCabinet.SaveFileAsync(key, fileStream, handleExisting);
                }
            });

            return await Task.WhenAll(saveTasks);
        }
    }
}

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

        public IProgress<WriteProgress> LocalFileUploadProgress { get; set; }
        public IProgress<WriteProgress> CabinetFileSaveProgress { get; set; }

        public async Task<ISaveResult[]> SaveInCabinet(HandleExistingMethod handleExisting, IFileScanner fileScanner = null) {

            var saveTasks = this.FileData.Select(async (fd) => {
                string key = this.keyProvider.GetKey(fd.Headers.ContentDisposition.FileName, fd.Headers.ContentType?.MediaType);

                if(String.IsNullOrWhiteSpace(key)) {
                    return new UploadSaveResult(key, "No key was provided");
                }

                string fileName = fd.LocalFileName;

                if (!File.Exists(fileName)) {
                    return new UploadSaveResult(key, "Could not find uploaded file.");
                }

                // File scanner to optionally check the file an remove if it's unsafe
                // note the c# 6 fileScanner?.ScanFileAsync syntax doesn't seem to work
                if (fileScanner != null) {
                    await fileScanner.ScanFileAsync(fileName);
                }

                if (!File.Exists(fileName)) {
                    return new UploadSaveResult(key, "File has been removed as it is unsafe.");
                }
                
                return await this.fileCabinet.SaveFileAsync(key, fileName, handleExisting, this.CabinetFileSaveProgress);
            });

            var saveResults = await Task.WhenAll(saveTasks);

            // cleanup temp files
            foreach(var file in this.FileData) {
                File.Delete(file.LocalFileName);
            }

            return saveResults;
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers) {
            var fileSize = headers.ContentDisposition.Size;
            var fileStream = base.GetStream(parent, headers);

            return new ProgressStream(fileStream, fileSize, this.LocalFileUploadProgress, disposeStream: true);
        }
    }
}

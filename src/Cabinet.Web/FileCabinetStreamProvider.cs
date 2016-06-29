using Cabinet.Core;
using Cabinet.Core.Progress;
using Cabinet.Core.Providers;
using Cabinet.Core.Results;
using Cabinet.Web.AntiVirus;
using Cabinet.Web.Files;
using Cabinet.Web.Results;
using Cabinet.Web.Validation;
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
        private readonly IUploadValidator fileValidator;
        private readonly IKeyProvider keyProvider;

        public FileCabinetStreamProvider(IFileCabinet fileCabinet, IUploadValidator fileValidator, IKeyProvider keyProvider, string tempFileFolder)
            : base(tempFileFolder) {
            Contract.NotNull(fileCabinet, nameof(fileCabinet));
            Contract.NotNull(fileValidator, nameof(fileValidator));
            Contract.NotNull(keyProvider, nameof(keyProvider));
            Contract.NotNullOrEmpty(tempFileFolder, nameof(tempFileFolder));

            this.fileCabinet = fileCabinet;
            this.fileValidator = fileValidator;
            this.keyProvider = keyProvider;
        }

        public IProgress<IWriteProgress> LocalFileUploadProgress { get; set; }
        public IProgress<IWriteProgress> CabinetFileSaveProgress { get; set; }

        public async Task<UploadSaveResult[]> SaveInCabinet(HandleExistingMethod handleExisting = HandleExistingMethod.Throw, IFileScanner fileScanner = null) {
            var saveTasks = this.FileData.Select(async (fd) => {
                string uploadFileName = fd.Headers.ContentDisposition.FileName?.Trim('"')?.Trim('\\');
                string uploadExtension = Path.GetExtension(uploadFileName)?.TrimStart('.');
                string uploadMediaType = fd.Headers.ContentType?.MediaType;

                if(!this.fileValidator.IsFileTypeWhitelisted(uploadExtension, uploadMediaType)) {
                    return new UploadSaveResult(uploadFileName, uploadMediaType, "The file type is not allowed");
                }

                string fileName = fd.LocalFileName;
                var uploadedFileInfo = new FileInfo(fileName);

                if(this.fileValidator.IsFileTooLarge(uploadedFileInfo.Length)) {
                    return new UploadSaveResult(uploadFileName, uploadMediaType, "The file is too large");
                }

                if(this.fileValidator.IsFileTooSmall(uploadedFileInfo.Length)) {
                    return new UploadSaveResult(uploadFileName, uploadMediaType, "The file is too small");
                }

                string delimiter = this.fileCabinet.GetKeyDelimiter();
                string key = this.keyProvider.GetKey(uploadFileName, uploadMediaType, delimiter);

                if(String.IsNullOrWhiteSpace(key)) {
                    return new UploadSaveResult(uploadFileName, uploadMediaType, "No key was provided");
                }
                
                if(!File.Exists(fileName)) {
                    return new UploadSaveResult(uploadFileName, uploadMediaType, "Could not find uploaded file.");
                }

                // File scanner to optionally check the file an remove if it's unsafe
                // note the c# 6 fileScanner?.ScanFileAsync syntax doesn't seem to work
                if(fileScanner != null) {
                    await fileScanner.ScanFileAsync(fileName);
                }

                if(!File.Exists(fileName)) {
                    return new UploadSaveResult(uploadFileName, uploadMediaType, "File has been removed as it is unsafe.");
                }

                var cabinetResult = await this.fileCabinet.SaveFileAsync(key, fileName, handleExisting, this.CabinetFileSaveProgress);

                return new UploadSaveResult(uploadFileName, uploadMediaType, cabinetResult);
            });

            var saveResults = await Task.WhenAll(saveTasks);

            // cleanup temp files
            foreach(var file in this.FileData) {
                File.Delete(file.LocalFileName);
            }

            return saveResults;
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers) {
            var fileName = headers.ContentDisposition.FileName;
            var fileSize = headers.ContentDisposition.Size;
            var fileStream = base.GetStream(parent, headers);

            return new ProgressStream(fileName, fileStream, fileSize, this.LocalFileUploadProgress, disposeStream: true);
        }
    }
}

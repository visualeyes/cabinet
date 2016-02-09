using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.Files {
    public class FileTypeProvider : IFileTypeProvider {
        private static readonly FileType[] FileTypes;

        public IEnumerable<IFileType> GetFileTypes() {
            return FileTypes;
        }


        static FileTypeProvider() {
            FileTypes = new FileType[] {

                new FileType(FileTypeCategory.Document, "text/plain", "txt"),
                new FileType(FileTypeCategory.Document, "text/csv", "csv"),
                new FileType(FileTypeCategory.Document, "text/rtf", "rtf"),
                new FileType(FileTypeCategory.Document, "application/pdf", "pdf",
                    altContentTypes: new string[] { "application/acrobat" }
                ),

                new FileType(FileTypeCategory.Document, "application/msword", new string[] { "doc", "dot" }),
                new FileType(FileTypeCategory.Document, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new string[] { "docx" }),
                new FileType(FileTypeCategory.Document, "application/vnd.ms-excel", "xls"),
                new FileType(FileTypeCategory.Document, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
                new FileType(FileTypeCategory.Document, "application/vnd.ms-powerpoint", "ppt"),
                new FileType(FileTypeCategory.Document, "application/vnd.openxmlformats-officedocument.presentationml.presentation", "pptx"),
                new FileType(FileTypeCategory.Document, "application/vnd.openxmlformats-officedocument.presentationml.slideshow", "ppsx"),
                new FileType(FileTypeCategory.Document, "application/vnd.ms-publisher", "pub"),
                new FileType(FileTypeCategory.Document, "application/vnd.openxmlformats-officedocument.wordprocessingml.template", "dotx"),
                
                new FileType(FileTypeCategory.Image, "image/bitmap", "bmp"),
                new FileType(FileTypeCategory.Image, "image/gif", "gif"),
                new FileType(FileTypeCategory.Image, "image/jpeg", new string[] { "jpg", "jpeg" },
                    altContentTypes: new string[] { "application/pjpeg" }
                ),
                new FileType(FileTypeCategory.Image, "image/png", "png",
                    altContentTypes: new string[] { "application/x-png" }
                ),
                new FileType(FileTypeCategory.Image, "image/vnd.microsoft.icon", "ico"),

                new FileType(FileTypeCategory.Audio, "audio/mp4", new string[] { "m4a", "aac", "mp4" }),
                new FileType(FileTypeCategory.Audio, "audio/mpeg", "mp3"),

                new FileType(FileTypeCategory.Video, "video/quicktime", "mov"),
                new FileType(FileTypeCategory.Video, "video/mp4", "m4v"),
                new FileType(FileTypeCategory.Video, "video/avi", "avi"),

                new FileType(FileTypeCategory.Archive, "application/zip", "zip", 
                    altContentTypes: new string[] { "application/x-zip-compressed" }
                ),
                new FileType(FileTypeCategory.Archive, "application/x-7z-compressed", "7z"),

                new FileType(FileTypeCategory.Html, "text/html", new string[] { "html" , "htm" }),
                new FileType(FileTypeCategory.Code, "text/xml", "xml"),
                new FileType(FileTypeCategory.Code, "application/javascript", "js", 
                    altContentTypes: new string[] { "text/javascript", "application/x-javascript" }
                ),
                new FileType(FileTypeCategory.Code, "text/css", "css"),
                new FileType(FileTypeCategory.Code, "application/x-shockwave-flash", "swf"),

            };
        }
    }
}

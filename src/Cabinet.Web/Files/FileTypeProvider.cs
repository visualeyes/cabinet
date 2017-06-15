using System.Collections.Generic;

namespace Cabinet.Web.Files {
    // Complete list of MIME types
    // https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Complete_list_of_MIME_types

    // Annotation of /httpd/httpd/branches/2.2.x/docs/conf/mime.types
    // http://svn.apache.org/viewvc/httpd/httpd/branches/2.2.x/docs/conf/mime.types?view=annotate

    // IANA is the official registry of MIME media types
    // https://www.iana.org/assignments/media-types/media-types.xhtml

    public class FileTypeProvider : IFileTypeProvider {
        private static readonly FileType[] FileTypes;

        public IEnumerable<IFileType> GetFileTypes() {
            return FileTypes;
        }


        static FileTypeProvider() {
            FileTypes = new FileType[] {

                new FileType(FileTypeCategory.Document, "Plain text", "text/plain", new[] { "txt", "text" }),

                new FileType(FileTypeCategory.Document, "Comma-separated values (CSV)", "text/csv", "csv", altContentTypes: new[] {
                    "text/comma-separated-values",
                    "application/csv",
                    "application/excel",
                    "application/vnd.ms-excel",
                    "application/vnd.msexcel"
                }),

                new FileType(FileTypeCategory.Document, "Rich Text Format (RTF)", "application/rtf", new string[] { "rtf", "rtx" }, altContentTypes: new[] {
                    "application/rtx"
                }),

                new FileType(FileTypeCategory.Document, "Microsoft Word", "application/msword", new[] { "doc", "dot" }),
                new FileType(FileTypeCategory.Document, "Microsoft Visio", "application/vnd.visio", "vsd"),
                new FileType(FileTypeCategory.Document, "Microsoft Publisher", "application/vnd.ms-publisher", "pub"),

                new FileType(FileTypeCategory.Document, "Adobe Portable Document Format (PDF)", "application/pdf", "pdf", altContentTypes: new[] {
                    "application/x-pdf",
                    "application/acrobat"
                }),


                new FileType(FileTypeCategory.Document, "Microsoft Excel", "application/vnd.ms-excel", "xls", altContentTypes: new[] {
                    "application/excel",
                    "application/x-excel",
                    "application/x-msexcel"
                }),

                new FileType(FileTypeCategory.Document, "Microsoft PowerPoint", "application/vnd.ms-powerpoint", new[] { "ppt", "pps" }, altContentTypes: new[] {
                    "application/mspowerpoint",
                    "application/powerpoint",
                    "application/x-mspowerpoint"
                }),

                new FileType(FileTypeCategory.Document, "Microsoft Excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
                new FileType(FileTypeCategory.Document, "Microsoft PowerPoint", "application/vnd.openxmlformats-officedocument.presentationml.presentation", "pptx"),
                new FileType(FileTypeCategory.Document, "Microsoft PowerPoint slide show", "application/vnd.openxmlformats-officedocument.presentationml.slideshow", "ppsx"),
                new FileType(FileTypeCategory.Document, "Microsoft Word", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx"),
                new FileType(FileTypeCategory.Document, "Microsoft Word document template", "application/vnd.openxmlformats-officedocument.wordprocessingml.template", "dotx"),

                new FileType(FileTypeCategory.Document, "OpenDocuemnt presentation document", "application/vnd.oasis.opendocument.presentation", "odp"),
                new FileType(FileTypeCategory.Document, "OpenDocuemnt spreadsheet document", "application/vnd.oasis.opendocument.spreadsheet", "ods"),
                new FileType(FileTypeCategory.Document, "OpenDocument text document", "application/vnd.oasis.opendocument.text", "odt"),

                new FileType(FileTypeCategory.Image, "Bitmap image", "image/bitmap", "bmp"),
                new FileType(FileTypeCategory.Image, "Graphics Interchange Format (GIF)", "image/gif", "gif"),
                new FileType(FileTypeCategory.Image, "Tagged Image File Format (TIFF)", "image/tiff", new[] { "tif", "tiff" }),
                new FileType(FileTypeCategory.Image, "JPEG image", "image/jpeg", new[] { "jpg", "jpeg" }, altContentTypes: new[] { "application/pjpeg" }),
                new FileType(FileTypeCategory.Image, "Portable Network Graphics", "image/png", "png", altContentTypes: new[] { "application/x-png" }),
                new FileType(FileTypeCategory.Image, "Icon format", "image/x-icon", "ico", altContentTypes: new[] { "image/vnd.microsoft.icon" }),

                new FileType(FileTypeCategory.Audio, "Musical Instrument Digital Interface (MIDI)", "audio/midi", new[] { "mid", "midi" }),
                new FileType(FileTypeCategory.Audio, "MPEG-4 audio", "audio/mp4", new[] { "m4a", "aac", "mp4" }),
                new FileType(FileTypeCategory.Audio, "MPEG audio", "audio/mpeg", "mp3"),
                new FileType(FileTypeCategory.Audio, "Waveform Audio Format", "audio/x-wav", "wav", altContentTypes: new[] {
                    "audio/wave",
                    "audio/wav",
                    "audio/x-pn-wav"
                }),

                new FileType(FileTypeCategory.Video, "Quicktime video", "video/quicktime", new[] { "qt", "mov" }, altContentTypes: new[] {
                    "video/raw",
                    "video/rtp-enc-aescm128",
                    "video/rtx","video/smpte292m",
                    "video/ulpfec",
                    "video/vc1",
                    "video/vnd.cctv"
                }),

                new FileType(FileTypeCategory.Video, "MPEG-4 video", "video/x-m4v", "m4v", altContentTypes: new[] { "video/mp4" }),
                new FileType(FileTypeCategory.Video, "Audio Video Interleave (AVI)", "video/avi", "avi"),
                new FileType(FileTypeCategory.Video, "MPEG video", "video/mpeg", "mpeg"),

                new FileType(FileTypeCategory.Archive, "7-zip archive", "application/x-7z-compressed", "7z"),
                new FileType(FileTypeCategory.Archive, "RAR archive", "application/x-rar-compressed", "rar"),
                new FileType(FileTypeCategory.Archive, "Java archive (JAR)", "application/java-archive", "jar"),
                new FileType(FileTypeCategory.Archive, "Tape archive (TAR)", "application/x-tar", "tar"),
                new FileType(FileTypeCategory.Archive, "ZIP archive", "application/zip", "zip", altContentTypes: new[] {
                    "application/x-zip-compressed"
                }),

                new FileType(FileTypeCategory.Html, "HyperText Markup Language (HTML)", "text/html", new[] { "html" , "htm" }),
                new FileType(FileTypeCategory.Html, "Extensible Hypertext Markup Language (XHTML)", "application/xhtml+xml", new[] { "xhtml" }),

                new FileType(FileTypeCategory.Code, "Extensible Markup Language (XML)", "text/xml", "xml"),
                new FileType(FileTypeCategory.Code, "Cascading Style Sheets (CSS)", "text/css", "css"),
                new FileType(FileTypeCategory.Code, "Small web format (SWF) / Adobe Flash document", "application/x-shockwave-flash", "swf"),
                new FileType(FileTypeCategory.Code, "JavaScript Object Notation (JSON)", "application/json", "json"),
                new FileType(FileTypeCategory.Code, "JavaScript (ECMAScript)", "application/javascript", "js", altContentTypes: new[] {
                    "text/javascript",
                    "application/x-javascript"
                }),
            };
        }
    }
}

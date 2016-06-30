using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Cabinet.S3.Results {
    internal class GetObjectResult {
        public long Size { get; internal set; }
        public HttpStatusCode StatusCode { get; internal set; }
        public DateTime LastModifiedUtc { get; internal set; }
    }
}

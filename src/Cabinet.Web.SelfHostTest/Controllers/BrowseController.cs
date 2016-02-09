using Cabinet.Core;
using Cabinet.Core.Providers;
using Cabinet.FileSystem;
using Cabinet.Web.SelfHostTest.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Cabinet.Web.SelfHostTest.Controllers {
    [RoutePrefix("browse")]
    public class BrowseController : ApiController {
        private readonly IFileCabinet fileCabinet;
        private readonly IKeyProvider keyProvider;

        public BrowseController(IFileCabinet fileCabinet, IKeyProvider keyProvider) {
            this.fileCabinet = fileCabinet;
            this.keyProvider = keyProvider;
        }

        [Route(""), HttpGet]
        public async Task<IHttpActionResult> Get(string keyPrefix = "", bool recursive = false) {
            var files = await fileCabinet.GetItemAsync(keyPrefix: keyPrefix, recursive: recursive);

            return this.Ok(files);
        }

        [Route("{key}"), HttpGet]
        public async Task<IHttpActionResult> Get(string key) {
            var file = await fileCabinet.GetItemAsync(key);

            return this.Ok(file);
        }
    }
}

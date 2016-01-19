using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3 {
    public class S3CabinetConfig : IS3CabinetConfig {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }

        public string BucketName { get; set; }
    }
}

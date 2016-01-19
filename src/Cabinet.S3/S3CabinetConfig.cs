using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3 {
    public class S3CabinetConfig : IS3CabinetConfig {
        public AmazonS3Config AmazonS3Config { get; set; }

        public string BucketName { get; set; }
    }
}

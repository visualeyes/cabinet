using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3 {
    public interface IAmazonS3ClientFactory {
        IAmazonS3 GetS3Client(AmazonS3CabinetConfig config);
    }
}

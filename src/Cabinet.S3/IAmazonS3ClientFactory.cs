using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.S3 {
    public interface IAmazonS3ClientFactory {
        IAmazonS3 GetS3Client(AmazonS3CabinetConfig config);
        ITransferUtility GetTransferUtility(IAmazonS3 client);
    }
}

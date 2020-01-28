using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using Amazon.S3.Model;

namespace hbkpclient
{
    class S3
    {
        public static void GetS3Itens()
        {
            AmazonS3Client client = new AmazonS3Client();

            ListObjectsRequest listRequest = new ListObjectsRequest
            {
                BucketName = "SampleBucket",
            };
        }
    }
}

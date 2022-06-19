using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.StorageServices;
using RESTClient;

namespace Assets.Scripts
{
    public class AzureClient
    {
        public static string storageAccount = "probastorageaccount";
        public static string accessKey = "03+Xn+3CQUB65LbcnJ4+lJCZIX6BsrRe4Iidq204XK8WtNVUlGf4VNjXMiJ3PjnCFD9bkKkQaGLtu1OZ8vUqzQ==";
        public static string container = "containerlicenta";
        public static StorageServiceClient client = StorageServiceClient.Create(storageAccount, accessKey);
        public static BlobService blobService = client.GetBlobService();


    }
}

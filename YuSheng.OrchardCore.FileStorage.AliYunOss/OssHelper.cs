using Aliyun.OSS;
using System.Collections.Generic;

namespace YuSheng.OrchardCore.FileStorage.AliYunOss
{
    public static class OssHelper
    {
        public static void DeleteMultiObjects(OssClient client, string bucketName, string objectName, List<string> keys)
        {
            ObjectListing listResult = client.ListObjects(bucketName);
            foreach (OssObjectSummary summary in listResult.ObjectSummaries)
            {
                keys.Add(summary.Key);
            }
            bool quietMode = false;
            DeleteObjectsRequest request = new DeleteObjectsRequest(bucketName, keys, quietMode);
            DeleteObjectsResult result = client.DeleteObjects(request);
        }
    }
}

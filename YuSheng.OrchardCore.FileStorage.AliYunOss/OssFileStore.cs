using Aliyun.OSS;
using OrchardCore.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YuSheng.OrchardCore.FileStorage.AliYunOss
{


    public class OssFileStore : IFileStore
    {
        private const string _directoryMarkerFileName = "OrchardCore.Media.txt";

        private readonly OssStorageOptions _options;

        private readonly OssClient _ossClient;

        public OssFileStore(OssStorageOptions options)
        {
            _options = options;
            _ossClient = new OssClient(_options.Endpoint, _options.AccessKeyId, _options.AccessKeySecret);
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {

            try
            {
                ObjectMetadata objectMetadata = _ossClient.GetObjectMetadata(_options.BucketName, path.StartsWith('/') ? path.Substring(1) : path);
                if (objectMetadata == null)
                {
                    return Task.FromResult<IFileStoreEntry>(null);
                }
                return Task.FromResult<IFileStoreEntry>(new OssFile(path, objectMetadata.ContentLength, objectMetadata.LastModified));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IFileStoreEntry>(null);
            }
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    //the root folder path
                    path = _options.BasePath + _directoryMarkerFileName;
                }
                else if (path.Contains(_directoryMarkerFileName))
                {
                    //this path is created folder path
                    //do nothing
                }
                else if (path.Split("/").FirstOrDefault() == _options.BasePath.Split("/").FirstOrDefault())
                {

                    if (path.LastOrDefault() == '/')
                    {
                        //cursor click the folder
                        path += _directoryMarkerFileName;
                    }
                    else
                    {
                        //created folder finished
                        path += "/" + _directoryMarkerFileName;
                    }
                }
                else
                {

                    path = _options.BasePath + path + "/" + _directoryMarkerFileName;
                }

                OssObject ossObject = _ossClient.GetObject(_options.BucketName, path);
                if (ossObject == null)
                {
                    return Task.FromResult<IFileStoreEntry>(null);
                }
                return Task.FromResult<IFileStoreEntry>(new OssDirectory(path, ossObject.Metadata.LastModified));
            }
            catch (Exception ex)
            {
                if (path.ToLower().Contains("_users") || path.ToLower().Contains("_users"))
                {
                    string objectContent = "this is text file tag";
                    byte[] binaryData = Encoding.ASCII.GetBytes(objectContent);
                    MemoryStream requestContent = new MemoryStream(binaryData);
                    PutObjectResult result = _ossClient.PutObject(_options.BucketName, path, requestContent);
                    if (result.HttpStatusCode == HttpStatusCode.OK)
                    {
                        return Task.FromResult<IFileStoreEntry>(new OssDirectory(path, DateTime.Now));
                    }
                }
                return Task.FromResult<IFileStoreEntry>(null);
            }

        }

        public IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = "", bool includeSubDirectories = false)
        {

            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = _options.BasePath;
                }
                List<IFileStoreEntry> list = new List<IFileStoreEntry>();
                List<OssFile> list2 = new List<OssFile>();
                List<OssDirectory> list3 = new List<OssDirectory>();
                ObjectListing objectListing = null;
                string marker = string.Empty;
                do
                {
                    ListObjectsRequest listObjectsRequest = new ListObjectsRequest(_options.BucketName)
                    {
                        Marker = marker,
                        MaxKeys = 100,
                        Delimiter = "/",
                        Prefix = path
                    };
                    objectListing = _ossClient.ListObjects(listObjectsRequest);
                    foreach (string current in objectListing.CommonPrefixes)
                    {
                        if (current.LastIndexOf("/") == current.Length - 1 && !current.Contains(_directoryMarkerFileName))
                        {
                            list3.Add(new OssDirectory(current));
                        }
                    }
                    foreach (OssObjectSummary current2 in objectListing.ObjectSummaries)
                    {
                        if (!(current2.Key == path))
                        {
                            string a = current2.Key.Split('/').LastOrDefault();
                            if (!a.Equals(_directoryMarkerFileName))
                            {
                                OssFile item = new OssFile(current2.Key, current2.Size, current2.LastModified);
                                list2.Add(item);
                            }
                        }
                    }
                    marker = objectListing.NextMarker;
                }
                while (objectListing.IsTruncated);
                list.AddRange(list3);
                list.AddRange(list2);
                return list.ToAsyncEnumerable();
            }
            catch (Exception)
            {
                return null;
            }

        }

        public Task<bool> TryCreateDirectoryAsync(string path)
        {

            if (_ossClient.DoesObjectExist(_options.BucketName, path))
            {
                throw new FileStoreException("Cannot create directory because the path '" + path + "' already exists and is a file.");
            }
            string filePath = (!path.Contains(_options.BasePath)) ? (_options.BasePath + path + "/" + _directoryMarkerFileName) : (path + "/" + _directoryMarkerFileName);
            try
            {
                string objectContent = "this is text file tag";
                byte[] binaryData = Encoding.ASCII.GetBytes(objectContent);
                MemoryStream requestContent = new MemoryStream(binaryData);
                PutObjectResult result = _ossClient.PutObject(_options.BucketName, filePath, requestContent);
                var re = result.HttpStatusCode == HttpStatusCode.OK;
                return Task.FromResult(re);
            }
            catch (Exception ex)
            {
                return Task.FromResult(false);

            }

        }

        public Task<bool> TryDeleteFileAsync(string path)
        {
            try
            {
                _ossClient.DeleteObject(_options.BucketName, path);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                return Task.FromResult(false);
            }

        }

        public Task<bool> TryDeleteDirectoryAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new FileStoreException("Cannot delete the root directory.");
            }
            string delimiter = "";
            try
            {
                List<IFileStoreEntry> list = new List<IFileStoreEntry>();
                List<OssFile> list2 = new List<OssFile>();
                List<OssDirectory> list3 = new List<OssDirectory>();
                ObjectListing objectListing = null;
                string marker = string.Empty;
                do
                {
                    ListObjectsRequest listObjectsRequest = new ListObjectsRequest(_options.BucketName)
                    {
                        Marker = marker,
                        MaxKeys = 100,
                        Delimiter = delimiter,
                        Prefix = path
                    };
                    objectListing = _ossClient.ListObjects(listObjectsRequest);
                    foreach (OssObjectSummary current in objectListing.ObjectSummaries)
                    {
                        if (current.Key.LastIndexOf("/") == current.Key.Length - 1)
                        {
                            list3.Add(new OssDirectory(current.Key, current.LastModified));
                        }
                        else
                        {
                            list2.Add(new OssFile(current.Key, current.Size, current.LastModified));
                        }
                    }
                    marker = objectListing.NextMarker;
                }
                while (objectListing.IsTruncated);
                list.AddRange(list3);
                list.AddRange(list2);
                foreach (OssFile current2 in list2)
                {
                    _ossClient.DeleteObject(_options.BucketName, current2.Path);
                }
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public Task MoveFileAsync(string oldPath, string newPath)
        {
            try
            {
                oldPath = oldPath.Replace("//", "/");
                newPath = newPath.Replace("//", "/");
                CopyObjectRequest copyObjectRequst = new CopyObjectRequest(_options.BucketName, oldPath, _options.BucketName, newPath);
                _ossClient.CopyObject(copyObjectRequst);
                _ossClient.DeleteObject(_options.BucketName, oldPath);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return Task.CompletedTask;

        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        {
            OssFile ossFile = fileStoreEntry as OssFile;

            return GetFileStreamAsync(ossFile.Path);
        }

        public Task<Stream> GetFileStreamAsync(string path)
        {
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            if (!_ossClient.DoesObjectExist(_options.BucketName, path))
            {
                throw new FileStoreException("Cannot get file stream because the file '" + path + "' does not exist.");
            }
            OssObject ossObject = _ossClient.GetObject(_options.BucketName, path);
            return Task.FromResult<Stream>(ossObject.Content);
        }

        public Task CopyFileAsync(string srcPath, string dstPath)
        {

            CopyObjectRequest copyObjectRequst = new CopyObjectRequest(_options.BucketName, srcPath, _options.BucketName, dstPath);
            _ossClient.CopyObject(copyObjectRequst);
            return Task.CompletedTask;
        }

        public Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        {
            try
            {
                string contentTypeStr = "application/octet-stream";
                ObjectMetadata objectMeta = new ObjectMetadata
                {
                    ContentType = contentTypeStr
                };
                List<string> strs = new List<string>();

                if (_ossClient.DoesObjectExist(_options.BucketName, path))
                {
                    throw new FileStoreException("Cannot create file '" + path + "' because it already exists.");
                }
                var objectName = path;
                // 定义上传的文件及所属存储空间的名称。您可以在InitiateMultipartUploadRequest中设置ObjectMeta，但不必指定其中的ContentLength。
                var request = new InitiateMultipartUploadRequest(_options.BucketName, objectName);
                var result = _ossClient.InitiateMultipartUpload(request);
                var uploadId = result.UploadId;
                // 计算分片总数。
                var partSize = 100 * 1024;//以100kb分片
                var fileSize = inputStream.Length;
                var partCount = fileSize / partSize;
                if (fileSize % partSize != 0)
                {
                    partCount++;
                }
                // 开始分片上传。PartETags是保存PartETag的列表，OSS收到用户提交的分片列表后，
                // 会逐一验证每个分片数据的有效性。当所有的数据分片通过验证后，
                // OSS会将这些分片组合成一个完整的文件。
                var partETags = new List<PartETag>();

                for (var i = 0; i < partCount; i++)
                {
                    var skipBytes = (long)partSize * i;
                    // 定位到本次上传的起始位置。
                    inputStream.Seek(skipBytes, 0);
                    // 计算本次上传的分片大小，最后一片为剩余的数据大小。
                    var size = (partSize < fileSize - skipBytes) ? partSize : (fileSize - skipBytes);
                    var requestPart = new UploadPartRequest(_options.BucketName, objectName, uploadId)
                    {
                        InputStream = inputStream,
                        PartSize = size,
                        PartNumber = i + 1
                    };
                    // 调用UploadPart接口执行上传功能，返回结果中包含了这个数据片的ETag值。
                    var resultOne = _ossClient.UploadPart(requestPart);
                    partETags.Add(resultOne.PartETag);
                    strs.Add($"finish {partETags.Count}/{partCount}");
                }

                var completeMultipartUploadRequest = new CompleteMultipartUploadRequest(_options.BucketName, objectName, uploadId);
                foreach (var partETag in partETags)
                {
                    completeMultipartUploadRequest.PartETags.Add(partETag);
                }
                var multipartUploadResult = _ossClient.CompleteMultipartUpload(completeMultipartUploadRequest);
                return Task.FromResult<string>(path);
            }
            catch (Exception)
            {
                return Task.FromResult<string>(string.Empty);
            }
        }
    }
}

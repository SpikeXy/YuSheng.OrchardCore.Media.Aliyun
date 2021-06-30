

// OrchardCore.FileStorage.AliYunOss.OssFile
using OrchardCore.FileStorage;
using System;
using System.Linq;

namespace YuSheng.OrchardCore.FileStorage.AliYunOss
{


    public class OssFile : IFileStoreEntry
    {
        private readonly string _path;

        private readonly string _name;

        private readonly string _directoryPath;

        private long _contentLength;

        private DateTime _lastModifiedTime;

        public string Path => _path;

        public string Name => _name;

        public string DirectoryPath => _directoryPath;

        public long Length => _contentLength;

        public DateTime LastModifiedUtc => _lastModifiedTime;

        public bool IsDirectory => false;

        public OssFile(string path, long contentLength, DateTime lastModifiedTime)
        {
            _path = path;
            _contentLength = contentLength;
            _lastModifiedTime = lastModifiedTime;
            _name = _path.Split('/').Last();
            if (_name == _path)
            {
                _directoryPath = "";
            }
            else
            {
                _directoryPath = _path.Substring(0, _path.Length - _name.Length - 1);
            }
        }
    }
}

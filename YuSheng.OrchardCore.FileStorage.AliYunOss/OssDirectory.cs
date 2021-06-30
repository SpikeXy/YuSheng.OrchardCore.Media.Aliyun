

// OrchardCore.FileStorage.AliYunOss.OssDirectory
using OrchardCore.FileStorage;
using System;

namespace YuSheng.OrchardCore.FileStorage.AliYunOss
{
    public class OssDirectory : IFileStoreEntry
    {
        private readonly string _path;

        private readonly DateTime _lastModifiedUtc = default(DateTime);

        private readonly string _name;

        private readonly string _directoryPath;

        public string Path => _path;

        public string Name => _name;

        public string DirectoryPath => _directoryPath;

        public long Length => 0L;

        public DateTime LastModifiedUtc => _lastModifiedUtc;

        public bool IsDirectory => true;

        public OssDirectory(string path, DateTime lastModifiedUtc = default(DateTime))
        {
            _path = path;
            _lastModifiedUtc = lastModifiedUtc;
            _name = _path.Split('/')[_path.Split('/').Length - 2];
            _directoryPath = ((_path.Length > _name.Length) ? _path.Substring(0, _path.Length - _name.Length - 1) : "");
        }
    }
}


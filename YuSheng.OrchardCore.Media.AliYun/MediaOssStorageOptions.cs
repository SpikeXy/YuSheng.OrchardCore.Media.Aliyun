using System;
using YuSheng.OrchardCore.FileStorage.AliYunOss;

namespace YuSheng.OrchardCore.Media.AliYun
{
    public class MediaOssStorageOptions : OssStorageOptions
    {
        [Obsolete("PublicHostName is obsolete. Use MediaOptions.CdnBaseUrl instead.")]
        public string PublicHostName
        {
            get;
            set;
        }
    }
}

using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "YuSheng AliYunOss Media",
    Author = "spike",
    Website = "https://github.com/SpikeXy/YuSheng.OrchardCore.Media.Aliyun",
    Version = "0.0.3",
    Description = "Enables support for storing media files in AliYun Oss Storage.",
    Dependencies = new[]
    {
        "OrchardCore.Media.Cache"
    },
    Category = "YuSheng Media"
)]

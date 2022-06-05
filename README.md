# YuSheng.OrchardCore.Media.Aliyun
aliyun oss orchardcore plugin

[![NuGet](https://img.shields.io/nuget/v/YuSheng.OrchardCore.Media.AliYun.svg)](https://www.nuget.org/packages/YuSheng.OrchardCore.Media.AliYun)


## resize_oss_url
resize image liquid filter
```
{{ "xxxxx/xxxxx/Program/useless-net-technology-01.jpg"  
   	| asset_url 
   	| resize_oss_url: 15  
   	| img_tag }}
```

## appsettings.json config demo
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "OrchardCore": {
    "OrchardCore.Media.AliYun": {
      "Endpoint": "xxxxxx", 
      "BucketName": "xxxxxxx",
      "BasePath": "xxxxxxxx/", // set the root folder , end with '/'
      "AccessKeyId": "xxxxxx",
      "AccessKeySecret": "xxxxx"
    }
  }
}

```

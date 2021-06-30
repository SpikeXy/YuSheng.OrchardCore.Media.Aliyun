# YuSheng.OrchardCore.Media.Aliyun
aliyun oss orchardcore plugin

[![NuGet](https://img.shields.io/nuget/v/YuSheng.OrchardCore.Media.AliYun.svg)](https://www.nuget.org/packages/YuSheng.OrchardCore.Media.AliYun)

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
    "OrchardCore_Media": {

      // The accepted sizes for custom width and height.
      // When the 'UseTokenizedQueryString' is True (default) all sizes are valid.
      "SupportedSizes": [ 16, 32, 50, 100, 160, 240, 480, 600, 1024, 2048 ],

      // The number of days to store images in the browser cache.
      // NB: To control cache headers for module static assets, refer to the Orchard Core Modules Section.
      "MaxBrowserCacheDays": 30,

      // The number of days a cached resized media item will be valid for, before being rebuilt on request.
      "MaxCacheDays": 365,

      // The maximum size of an uploaded file in bytes. 
      // NB: You might still need to configure the limit in IIS (https://docs.microsoft.com/en-us/iis/configuration/system.webserver/security/requestfiltering/requestlimits/)
      "MaxFileSize": 30000000,

      // A CDN base url that will be prefixed to the request path when serving images.
      //"CdnBaseUrl": "https://your-cdn.com",

      // The path used when serving media assets.
      "AssetsRequestPath": "/media",

      // The path used to store media assets. The path can be relative to the tenant's App_Data folder, or absolute.
      "AssetsPath": "Media",

      // Whether to use a token in the query string to prevent disc filling.
      //"UseTokenizedQueryString": true,

      // The list of allowed file extensions
      "AllowedFileExtensions": [

        // Images
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".ico",
        ".svg",

        // Documents
        ".pdf", // Portable Document Format; Adobe Acrobat
        ".doc", // Microsoft Word Document
        ".docx",
        ".ppt", // Microsoft PowerPoint Presentation
        ".pptx",
        ".pps",
        ".ppsx",
        ".odt", // OpenDocument Text Document
        ".xls", // Microsoft Excel Document
        ".xlsx",
        ".psd", // Adobe Photoshop Document

        // Audio
        ".mp3",
        ".m4a",
        ".ogg",
        ".wav",

        // Video
        ".mp4", // MPEG-4
        ".m4v",
        ".mov", // QuickTime
        ".wmv", // Windows Media Video
        ".avi",
        ".mpg",
        ".ogv", // Ogg
        ".3gp" // 3GPP
      ],

      // The Content Security Policy to apply to assets served from the media library.
      "ContentSecurityPolicy": "default-src 'self'; style-src 'unsafe-inline'"
    },
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


## Init aliyun oss folder
Creat a new txt file named 'OrchardCore.Media.txt',then upload OrchardCore.Media.txt to the BasePath folder(the aliyun cos folder you set in appsettings.json),now you can see the folder in cms media library when you enable this plugin.(You have to make sure that your OSS settings are correct,please take a look at aliyun oss documentation.)

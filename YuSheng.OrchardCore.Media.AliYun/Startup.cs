using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage;
using YuSheng.OrchardCore.FileStorage.AliYunOss;
using OrchardCore.Media.Core;
//using OrchardCore.Media.Events;
using YuSheng.OrchardCore.Media.AliYun.Filters;
using OrchardCore.Liquid;
using OrchardCore.Media;
using OrchardCore.Modules;
using OrchardCore.Media.Events;

namespace YuSheng.OrchardCore.Media.AliYun
{
    public class Startup : StartupBase
    {
        private readonly ILogger<Startup> _logger;
        private readonly IShellConfiguration _configuration;

        public Startup(ILogger<Startup> logger, IShellConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override int Order => 10;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddLiquidFilter<ResizeOssUrlFilter>("resize_oss_url");

            services.Configure<MediaOssStorageOptions>(_configuration.GetSection("OrchardCore.Media.AliYun"));

            // Only replace default implementation if options are valid.
            var connectionString = _configuration[$"OrchardCore.Media.AliYun:{nameof(MediaOssStorageOptions.Endpoint)}"];
            var containerName = _configuration[$"OrchardCore.Media.AliYun:{nameof(MediaOssStorageOptions.BucketName)}"];
            if (CheckOptions(connectionString, containerName, _logger))
            {
                // Register a media cache file provider.
                services.AddSingleton<IMediaFileStoreCacheFileProvider>(serviceProvider =>
                {
                    var hostingEnvironment = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();

                    if (String.IsNullOrWhiteSpace(hostingEnvironment.WebRootPath))
                    {
                        throw new Exception("The wwwroot folder for serving cache media files is missing.");
                    }

                    var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
                    var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                    var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                    var logger = serviceProvider.GetRequiredService<ILogger<DefaultMediaFileStoreCacheFileProvider>>();

                    var mediaCachePath = GetMediaCachePath(hostingEnvironment, DefaultMediaFileStoreCacheFileProvider.AssetsCachePath, shellSettings);

                    if (!Directory.Exists(mediaCachePath))
                    {
                        Directory.CreateDirectory(mediaCachePath);
                    }

                    return new DefaultMediaFileStoreCacheFileProvider(logger, mediaOptions.AssetsRequestPath, mediaCachePath);
                });

                // Replace the default media file provider with the media cache file provider.
                services.Replace(ServiceDescriptor.Singleton<IMediaFileProvider>(serviceProvider =>
                    serviceProvider.GetRequiredService<IMediaFileStoreCacheFileProvider>()));

                // Register the media cache file provider as a file store cache provider.
                services.AddSingleton<IMediaFileStoreCache>(serviceProvider =>
                    serviceProvider.GetRequiredService<IMediaFileStoreCacheFileProvider>());

                // Replace the default media file store with a blob file store.
                services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
                {
                    var ossStorageOptions = serviceProvider.GetRequiredService<IOptions<MediaOssStorageOptions>>().Value;
                    var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                    var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                    var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
                    var clock = serviceProvider.GetRequiredService<IClock>();
                    //var contentTypeProvider = serviceProvider.GetRequiredService<IContentTypeProvider>();
                    var fileStore = new OssFileStore(ossStorageOptions);
                    var mediaEventHandlers = serviceProvider.GetServices<IMediaEventHandler>();
                    var mediaCreatingEventHandlers = serviceProvider.GetServices<IMediaCreatingEventHandler>();
                    var logger = serviceProvider.GetRequiredService<ILogger<DefaultMediaFileStore>>();

                    var mediaPath = GetMediaPath(shellOptions.Value, shellSettings, mediaOptions.AssetsPath);

                    var mediaUrlBase = "/" + fileStore.Combine(shellSettings.RequestUrlPrefix, mediaOptions.AssetsRequestPath);

                    var originalPathBase = serviceProvider.GetRequiredService<IHttpContextAccessor>()
                        .HttpContext?.Features.Get<ShellContextFeature>()?.OriginalPathBase ?? null;

                    if (originalPathBase.HasValue)
                    {
                        mediaUrlBase = fileStore.Combine(originalPathBase.Value, mediaUrlBase);
                    }
                    return new DefaultMediaFileStore(fileStore, mediaUrlBase, mediaOptions.CdnBaseUrl, mediaEventHandlers, mediaCreatingEventHandlers, logger);
                    //return new DefaultMediaFileStore(fileStore, mediaUrlBase, mediaOptions.CdnBaseUrl);

                }));
            }
        }

        private string GetMediaPath(ShellOptions shellOptions, ShellSettings shellSettings, string assetsPath)
        {
            return PathExtensions.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, assetsPath);
        }

        private string GetMediaCachePath(Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment, string assetsPath, ShellSettings shellSettings)
        {
            return PathExtensions.Combine(hostingEnvironment.WebRootPath, assetsPath, shellSettings.Name);
        }

        private static bool CheckOptions(string connectionString, string containerName, ILogger logger)
        {
            var optionsAreValid = true;

            if (String.IsNullOrWhiteSpace(connectionString))
            {
                logger.LogError("AliYun Oss Storage is enabled but not active because the 'ConnectionString' is missing or empty in application configuration.");
                optionsAreValid = false;
            }

            if (String.IsNullOrWhiteSpace(containerName))
            {
                logger.LogError("AliYun Oss Storage is enabled but not active because the 'ContainerName' is missing or empty in application configuration.");
                optionsAreValid = false;
            }

            return optionsAreValid;
        }
    }
}

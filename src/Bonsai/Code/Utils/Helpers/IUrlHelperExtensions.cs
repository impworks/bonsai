using System;
using Bonsai.Areas.Front.ViewModels.Media;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Code.Utils.Helpers;

/// <summary>
/// Helper methods for augmenting IUrlHelper.
/// </summary>
public static class IUrlHelperExtensions
{
    /// <summary>
    /// Returns a cache-friendly media URL.
    /// </summary>
    public static string MediaUrl(this IUrlHelper helper, MediaThumbnailVM media)
    {
        var url = helper.Content(media.ThumbnailUrl);
            
        if (!media.IsProcessed)
            url += "?nonce=" + Guid.NewGuid().ToString("N")[..10];
            
        return url;
    }
        
    /// <summary>
    /// Returns a cache-friendly media URL.
    /// </summary>
    public static string MediaUrl(this IUrlHelper helper, MediaVM media)
    {
        var url = helper.Content(media.PreviewPath);
            
        if (!media.IsProcessed)
            url += "?nonce=" + Guid.NewGuid().ToString("N")[..10];
            
        return url;
    }
}
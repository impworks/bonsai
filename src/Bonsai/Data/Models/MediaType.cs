using System;

namespace Bonsai.Data.Models;

/// <summary>
/// Type of the uploaded media file.
/// </summary>
public enum MediaType
{
    /// <summary>
    /// Static photo.
    /// </summary>
    Photo,

    /// <summary>
    /// Photosphere.
    /// </summary>
    [Obsolete("Not yet implemented")]
    Photo360,

    /// <summary>
    /// Video.
    /// </summary>
    Video,

    /// <summary>
    /// PDF or other file.
    /// </summary>
    Document
}
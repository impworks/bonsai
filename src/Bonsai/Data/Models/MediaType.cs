using System;
using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Type of the uploaded media file.
    /// </summary>
    public enum MediaType
    {
        [Description("Фото")]
        Photo,

        [Description("Фотосфера")]
        [Obsolete("Not yet implemented")]
        Photo360,

        [Description("Видео")]
        Video,

        [Description("Документ")]
        Document
    }
}

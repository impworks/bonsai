using System;

namespace Bonsai.Areas.Admin.ViewModels.Common
{
    /// <summary>
    /// Request arguments for picking a page/media.
    /// </summary>
    public class PickRequestVM<T> where T: Enum
    {
        public string Query { get; set; }
        public int? Count { get; set; }
        public int? Offset { get; set; }
        public T[] Types { get; set; }
    }
}

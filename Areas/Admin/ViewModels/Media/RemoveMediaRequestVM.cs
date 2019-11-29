using System;

namespace Bonsai.Areas.Admin.ViewModels.Media
{
    public class RemoveMediaRequestVM
    {
        public Guid Id { get; set; }
        public bool RemoveFile { get; set; }
    }
}

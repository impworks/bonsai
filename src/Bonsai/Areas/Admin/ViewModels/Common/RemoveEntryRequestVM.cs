using System;

namespace Bonsai.Areas.Admin.ViewModels.Common
{
    public class RemoveEntryRequestVM
    {
        public Guid Id { get; set; }
        public bool RemoveCompletely { get; set; }
    }
}
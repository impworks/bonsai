using System;

namespace Bonsai.Data.Models
{
    public class Media
    {
        public Guid Id { get; set; }

        public string FilePath { get; set; }
        public string FileName { get; set; }

        public string Facts { get; set; }
    }
}

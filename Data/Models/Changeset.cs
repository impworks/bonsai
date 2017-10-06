using System;

namespace Bonsai.Data.Models
{
    public class Changeset
    {
        public Guid Id { get; set; }

        public DateTimeOffset Date { get; set; }
        public AppUser Author { get; set; }

        public Guid SourceEntityId { get; set; }
        public string SourceDiff { get; set; }

        public string AffectedEntityIds { get; set; }
        public string AffectedDiff { get; set; }
    }
}

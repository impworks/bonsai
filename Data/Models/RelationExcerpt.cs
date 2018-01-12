using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Basic information about a relation between two pages.
    /// </summary>
    [NotMapped]
    [DebuggerDisplay("{Type}: {SourceId} -> {DestinationId} ({Duration})")]
    public class RelationExcerpt
    {
        public Guid SourceId { get; set; }
        public Guid DestinationId { get; set; }
        public RelationType Type { get; set; }
        public string Duration { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Basic information about a page.
    /// </summary>
    [NotMapped]
    [DebuggerDisplay("{Title} ({Id})")]
    public class PageExcerpt: IEquatable<PageExcerpt>
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Key { get; set; }
        public PageType PageType { get; set; }
        public bool? Gender { get; set; }
        public string BirthDate { get; set; }
        public string DeathDate { get; set; }
        public string ShortName { get; set; }

        #region Equality members (auto-generated)

        public bool Equals(PageExcerpt other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PageExcerpt) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(PageExcerpt left, PageExcerpt right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PageExcerpt left, PageExcerpt right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}

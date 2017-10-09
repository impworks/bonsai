using Bonsai.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Data
{
    /// <summary>
    /// Main data context of the application.
    /// </summary>
    public class AppDbContext: IdentityDbContext<AppUser>
    {
        public virtual DbSet<AccessRule> AccessRules => Set<AccessRule>();
        public virtual DbSet<Changeset> Changes => Set<Changeset>();
        public virtual DbSet<Media> Media => Set<Media>();
        public virtual DbSet<Page> Pages => Set<Page>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                   .HasMany(x => x.Changes).WithOne(x => x.Author);

            builder.Entity<AccessRule>()
                   .HasOne(x => x.Page).WithMany();

            builder.Entity<AccessRule>()
                .HasOne(x => x.User).WithMany();
        }
    }
}

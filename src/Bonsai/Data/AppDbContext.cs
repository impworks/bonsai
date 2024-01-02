using System;
using System.Linq;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bonsai.Data
{
    /// <summary>
    /// Main data context of the application.
    /// </summary>
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        public virtual DbSet<DynamicConfigWrapper> DynamicConfig => Set<DynamicConfigWrapper>();
        public virtual DbSet<Changeset> Changes => Set<Changeset>();
        public virtual DbSet<ChangeEventGroup> ChangeEvents => Set<ChangeEventGroup>();
        public virtual DbSet<LivingBeingOverview> LivingBeingOverviews => Set<LivingBeingOverview>();
        public virtual DbSet<Media> Media => Set<Media>();
        public virtual DbSet<MediaTag> MediaTags => Set<MediaTag>();

        public virtual DbSet<JobState> JobStates => Set<JobState>();
        public virtual DbSet<Page> Pages => Set<Page>();
        public virtual DbSet<PageAlias> PageAliases => Set<PageAlias>();
        public virtual DbSet<PageScored> PagesScored => Set<PageScored>();
        public virtual DbSet<Relation> Relations => Set<Relation>();
        public virtual DbSet<PageDraft> PageDrafts => Set<PageDraft>();
        public virtual DbSet<PageReference> PageReferences => Set<PageReference>();
        public virtual DbSet<TreeLayout> TreeLayouts => Set<TreeLayout>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>().HasMany(x => x.Changes).WithOne(x => x.Author).HasForeignKey(x => x.AuthorId);
            builder.Entity<AppUser>().HasOne(x => x.Page).WithMany().IsRequired(false).HasForeignKey(x => x.PageId);

            builder.Entity<Changeset>().HasOne(x => x.Author).WithMany();

            builder.Entity<Page>().HasIndex(x => x.Key).IsUnique(true);
            builder.Entity<Page>().HasIndex(x => x.IsDeleted).IsUnique(false);
            builder.Entity<Page>().HasMany(x => x.Aliases).WithOne(x => x.Page).HasForeignKey(x => x.PageId).IsRequired();
            builder.Entity<Page>().HasOne(x => x.MainPhoto).WithMany().IsRequired(false).HasForeignKey(x => x.MainPhotoId);
            builder.Entity<Page>().HasOne(x => x.TreeLayout).WithMany().IsRequired(false).HasForeignKey(x => x.TreeLayoutId);
            builder.Entity<Page>().HasOne(x => x.LivingBeingOverview).WithOne().HasForeignKey<LivingBeingOverview>(x => x.PageId).IsRequired(false);

            builder.Entity<PageAlias>().HasIndex(x => x.Key).IsUnique(true);

            builder.Entity<PageDraft>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).IsRequired(true);
            builder.Entity<PageDraft>().HasIndex(x => x.PageId).IsUnique(false);

            builder.Entity<Relation>().HasOne(x => x.Source).WithMany(x => x.Relations).HasForeignKey(x => x.SourceId);
            builder.Entity<Relation>().HasOne(x => x.Destination).WithMany().HasForeignKey(x => x.DestinationId);
            builder.Entity<Relation>().HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId);
            builder.Entity<Relation>().HasIndex(x => x.IsComplementary).IsUnique(false);
            builder.Entity<Relation>().HasIndex(x => x.IsDeleted).IsUnique(false);

            builder.Entity<Media>().HasOne(x => x.Uploader).WithMany().IsRequired(false);
            builder.Entity<Media>().HasIndex(x => x.Key).IsUnique(true);
            builder.Entity<Media>().HasIndex(x => x.IsDeleted).IsUnique(false);

            builder.Entity<MediaTag>().HasOne(x => x.Media).WithMany(x => x.Tags).HasForeignKey(x => x.MediaId);
            builder.Entity<MediaTag>().HasOne(x => x.Object).WithMany(x => x.MediaTags).HasForeignKey(x => x.ObjectId);

            builder.Entity<PageScored>().ToView("PagesScored");
            builder.Entity<PageScored>().HasOne(x => x.MainPhoto).WithMany().IsRequired(false).HasForeignKey(x => x.MainPhotoId);

            builder.Entity<ChangeEventGroup>().ToView("ChangesGrouped");

            builder.Entity<PageReference>().HasOne(x => x.Source).WithMany().HasForeignKey(x => x.SourceId);
            builder.Entity<PageReference>().HasOne(x => x.Destination).WithMany(x => x.References).HasForeignKey(x => x.DestinationId);

            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                ApplyDataTimeOffsetConverter(builder);
        }

        private void ApplyDataTimeOffsetConverter(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType
                                           .GetProperties()
                                           .Where(p => p.PropertyType == typeof(DateTimeOffset)
                                                       || p.PropertyType == typeof(DateTimeOffset?));

                foreach (var property in properties)
                {
                    builder.Entity(entityType.Name)
                           .Property(property.Name)
                           .HasConversion(new DateTimeOffsetToStringConverter());
                }
            }
        }
    }
}

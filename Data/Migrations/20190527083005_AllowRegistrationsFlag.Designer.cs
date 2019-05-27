﻿// <auto-generated />
using System;
using Bonsai.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bonsai.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20190527083005_AllowRegistrationsFlag")]
    partial class AllowRegistrationsFlag
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.2-rtm-30932")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Bonsai.Data.Models.AppConfig", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("AllowGuests");

                    b.Property<bool>("AllowRegistration");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.ToTable("Config");
                });

            modelBuilder.Entity("Bonsai.Data.Models.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("Birthday")
                        .HasMaxLength(10);

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName")
                        .HasMaxLength(256);

                    b.Property<bool>("IsValidated");

                    b.Property<string>("LastName")
                        .HasMaxLength(256);

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("MiddleName")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<Guid?>("PageId");

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.HasIndex("PageId");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Changeset", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AppUserId");

                    b.Property<string>("AuthorId")
                        .IsRequired();

                    b.Property<DateTimeOffset>("Date");

                    b.Property<Guid?>("EditedMediaId");

                    b.Property<Guid?>("EditedPageId");

                    b.Property<Guid?>("EditedRelationId");

                    b.Property<Guid?>("GroupId");

                    b.Property<string>("OriginalState");

                    b.Property<Guid?>("RevertedChangesetId");

                    b.Property<int>("Type");

                    b.Property<string>("UpdatedState");

                    b.HasKey("Id");

                    b.HasIndex("AppUserId");

                    b.HasIndex("AuthorId");

                    b.HasIndex("EditedMediaId");

                    b.HasIndex("EditedPageId");

                    b.HasIndex("EditedRelationId");

                    b.ToTable("Changes");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Media", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Date")
                        .HasMaxLength(30);

                    b.Property<string>("Description");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasMaxLength(300);

                    b.Property<bool>("IsDeleted");

                    b.Property<bool>("IsProcessed");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<string>("Title");

                    b.Property<int>("Type");

                    b.Property<DateTimeOffset>("UploadDate");

                    b.Property<string>("UploaderId");

                    b.HasKey("Id");

                    b.HasIndex("IsDeleted");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.HasIndex("UploaderId");

                    b.ToTable("Media");
                });

            modelBuilder.Entity("Bonsai.Data.Models.MediaEncodingJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("MediaId");

                    b.HasKey("Id");

                    b.HasIndex("MediaId")
                        .IsUnique();

                    b.ToTable("MediaJobs");
                });

            modelBuilder.Entity("Bonsai.Data.Models.MediaTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Coordinates")
                        .HasMaxLength(100);

                    b.Property<Guid>("MediaId");

                    b.Property<Guid?>("ObjectId");

                    b.Property<string>("ObjectTitle");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.HasIndex("MediaId");

                    b.HasIndex("ObjectId");

                    b.ToTable("MediaTags");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Page", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreationDate");

                    b.Property<string>("Description");

                    b.Property<string>("Facts");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<DateTimeOffset>("LastUpdateDate");

                    b.Property<Guid?>("MainPhotoId");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<Guid?>("TreeLayoutId");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.HasIndex("IsDeleted");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.HasIndex("MainPhotoId");

                    b.HasIndex("TreeLayoutId");

                    b.ToTable("Pages");
                });

            modelBuilder.Entity("Bonsai.Data.Models.PageAlias", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<int>("Order");

                    b.Property<Guid?>("PageId")
                        .IsRequired();

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.HasIndex("PageId");

                    b.ToTable("PageAliases");
                });

            modelBuilder.Entity("Bonsai.Data.Models.PageDraft", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content")
                        .IsRequired();

                    b.Property<DateTimeOffset>("LastUpdateDate");

                    b.Property<Guid?>("PageId");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("PageId");

                    b.HasIndex("UserId");

                    b.ToTable("PageDrafts");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Relation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DestinationId");

                    b.Property<string>("Duration")
                        .HasMaxLength(30);

                    b.Property<Guid?>("EventId");

                    b.Property<bool>("IsComplementary");

                    b.Property<bool>("IsDeleted");

                    b.Property<Guid>("SourceId");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.HasIndex("DestinationId");

                    b.HasIndex("EventId");

                    b.HasIndex("IsComplementary");

                    b.HasIndex("IsDeleted");

                    b.HasIndex("SourceId");

                    b.ToTable("Relations");
                });

            modelBuilder.Entity("Bonsai.Data.Models.TreeLayout", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("GenerationDate");

                    b.Property<string>("LayoutJson");

                    b.HasKey("Id");

                    b.ToTable("TreeLayouts");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Bonsai.Data.Models.AppUser", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Page", "Page")
                        .WithMany()
                        .HasForeignKey("PageId");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Changeset", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser")
                        .WithMany("Changes")
                        .HasForeignKey("AppUserId");

                    b.HasOne("Bonsai.Data.Models.AppUser", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Bonsai.Data.Models.Media", "EditedMedia")
                        .WithMany()
                        .HasForeignKey("EditedMediaId");

                    b.HasOne("Bonsai.Data.Models.Page", "EditedPage")
                        .WithMany()
                        .HasForeignKey("EditedPageId");

                    b.HasOne("Bonsai.Data.Models.Relation", "EditedRelation")
                        .WithMany()
                        .HasForeignKey("EditedRelationId");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Media", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser", "Uploader")
                        .WithMany()
                        .HasForeignKey("UploaderId");
                });

            modelBuilder.Entity("Bonsai.Data.Models.MediaEncodingJob", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Media", "Media")
                        .WithOne()
                        .HasForeignKey("Bonsai.Data.Models.MediaEncodingJob", "MediaId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Bonsai.Data.Models.MediaTag", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Media", "Media")
                        .WithMany("Tags")
                        .HasForeignKey("MediaId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Bonsai.Data.Models.Page", "Object")
                        .WithMany("MediaTags")
                        .HasForeignKey("ObjectId");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Page", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Media", "MainPhoto")
                        .WithMany()
                        .HasForeignKey("MainPhotoId");

                    b.HasOne("Bonsai.Data.Models.TreeLayout", "TreeLayout")
                        .WithMany()
                        .HasForeignKey("TreeLayoutId");
                });

            modelBuilder.Entity("Bonsai.Data.Models.PageAlias", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Page", "Page")
                        .WithMany("Aliases")
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Bonsai.Data.Models.PageDraft", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Bonsai.Data.Models.Relation", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Page", "Destination")
                        .WithMany()
                        .HasForeignKey("DestinationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Bonsai.Data.Models.Page", "Event")
                        .WithMany()
                        .HasForeignKey("EventId");

                    b.HasOne("Bonsai.Data.Models.Page", "Source")
                        .WithMany("Relations")
                        .HasForeignKey("SourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Bonsai.Data.Models.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}

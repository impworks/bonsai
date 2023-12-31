﻿// <auto-generated />
using System;
using Bonsai.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bonsai.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20231230155428_LivingBeingOverviewRename")]
    partial class LivingBeingOverviewRename
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Bonsai.Data.Models.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<int>("AuthType")
                        .HasColumnType("integer");

                    b.Property<string>("Birthday")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("FirstName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("IsValidated")
                        .HasColumnType("boolean");

                    b.Property<string>("LastName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("MiddleName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<Guid?>("PageId")
                        .HasColumnType("uuid");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.HasIndex("PageId");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Bonsai.Data.Models.ChangeEventGroup", b =>
                {
                    b.Property<string>("GroupKey")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Ids")
                        .HasColumnType("text");

                    b.HasKey("GroupKey");

                    b.ToTable((string)null);

                    b.ToView("ChangesGrouped", (string)null);
                });

            modelBuilder.Entity("Bonsai.Data.Models.Changeset", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AppUserId")
                        .HasColumnType("text");

                    b.Property<string>("AuthorId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ChangeType")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("EditedMediaId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("EditedPageId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("EditedRelationId")
                        .HasColumnType("uuid");

                    b.Property<int>("EntityType")
                        .HasColumnType("integer");

                    b.Property<Guid?>("GroupId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("RevertedChangesetId")
                        .HasColumnType("uuid");

                    b.Property<string>("UpdatedState")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AppUserId");

                    b.HasIndex("AuthorId");

                    b.HasIndex("EditedMediaId");

                    b.HasIndex("EditedPageId");

                    b.HasIndex("EditedRelationId");

                    b.ToTable("Changes");
                });

            modelBuilder.Entity("Bonsai.Data.Models.DynamicConfigWrapper", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DynamicConfig");
                });

            modelBuilder.Entity("Bonsai.Data.Models.JobState", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Arguments")
                        .HasColumnType("text");

                    b.Property<string>("ArgumentsType")
                        .HasColumnType("text");

                    b.Property<DateTime?>("FinishDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool?>("Success")
                        .HasColumnType("boolean");

                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("JobStates");
                });

            modelBuilder.Entity("Bonsai.Data.Models.LivingBeingOverview", b =>
                {
                    b.Property<Guid>("PageId")
                        .HasColumnType("uuid");

                    b.Property<string>("BirthDate")
                        .HasColumnType("text");

                    b.Property<string>("DeathDate")
                        .HasColumnType("text");

                    b.Property<bool?>("Gender")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDead")
                        .HasColumnType("boolean");

                    b.Property<string>("MaidenName")
                        .HasColumnType("text");

                    b.Property<string>("ShortName")
                        .HasColumnType("text");

                    b.HasKey("PageId");

                    b.ToTable("LivingBeingOverviews");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Media", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Date")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsProcessed")
                        .HasColumnType("boolean");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("UploadDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UploaderId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("IsDeleted");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.HasIndex("UploaderId");

                    b.ToTable("Media");
                });

            modelBuilder.Entity("Bonsai.Data.Models.MediaTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Coordinates")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<Guid>("MediaId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ObjectId")
                        .HasColumnType("uuid");

                    b.Property<string>("ObjectTitle")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MediaId");

                    b.HasIndex("ObjectId");

                    b.ToTable("MediaTags");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Page", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Facts")
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTimeOffset>("LastUpdateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("MainPhotoId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<Guid?>("TreeLayoutId")
                        .HasColumnType("uuid");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

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
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<Guid>("PageId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("Id");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.HasIndex("PageId");

                    b.ToTable("PageAliases");
                });

            modelBuilder.Entity("Bonsai.Data.Models.PageDraft", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("LastUpdateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("PageId")
                        .HasColumnType("uuid");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("PageId");

                    b.HasIndex("UserId");

                    b.ToTable("PageDrafts");
                });

            modelBuilder.Entity("Bonsai.Data.Models.PageReference", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DestinationId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SourceId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("DestinationId");

                    b.HasIndex("SourceId");

                    b.ToTable("PageReferences");
                });

            modelBuilder.Entity("Bonsai.Data.Models.PageScored", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<int>("CompletenessScore")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("HasAnimalName")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasAnimalSpecies")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasBirthPlace")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasBirthday")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasEventDate")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasGender")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasHumanName")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasLocationAddress")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasPhoto")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasRelations")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasText")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Key")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("LastUpdateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("MainPhotoId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MainPhotoId");

                    b.ToTable((string)null);

                    b.ToView("PagesScored", (string)null);
                });

            modelBuilder.Entity("Bonsai.Data.Models.Relation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DestinationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Duration")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<Guid?>("EventId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsComplementary")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("SourceId")
                        .HasColumnType("uuid");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

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
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("GenerationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LayoutJson")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("TreeLayouts");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Bonsai.Data.Models.AppUser", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Page", "Page")
                        .WithMany()
                        .HasForeignKey("PageId");

                    b.Navigation("Page");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Changeset", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser", null)
                        .WithMany("Changes")
                        .HasForeignKey("AppUserId");

                    b.HasOne("Bonsai.Data.Models.AppUser", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Bonsai.Data.Models.Media", "EditedMedia")
                        .WithMany()
                        .HasForeignKey("EditedMediaId");

                    b.HasOne("Bonsai.Data.Models.Page", "EditedPage")
                        .WithMany()
                        .HasForeignKey("EditedPageId");

                    b.HasOne("Bonsai.Data.Models.Relation", "EditedRelation")
                        .WithMany()
                        .HasForeignKey("EditedRelationId");

                    b.Navigation("Author");

                    b.Navigation("EditedMedia");

                    b.Navigation("EditedPage");

                    b.Navigation("EditedRelation");
                });

            modelBuilder.Entity("Bonsai.Data.Models.LivingBeingOverview", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Page", null)
                        .WithOne("LivingBeingOverview")
                        .HasForeignKey("Bonsai.Data.Models.LivingBeingOverview", "PageId");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Media", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser", "Uploader")
                        .WithMany()
                        .HasForeignKey("UploaderId");

                    b.Navigation("Uploader");
                });

            modelBuilder.Entity("Bonsai.Data.Models.MediaTag", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Media", "Media")
                        .WithMany("Tags")
                        .HasForeignKey("MediaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Bonsai.Data.Models.Page", "Object")
                        .WithMany("MediaTags")
                        .HasForeignKey("ObjectId");

                    b.Navigation("Media");

                    b.Navigation("Object");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Page", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Media", "MainPhoto")
                        .WithMany()
                        .HasForeignKey("MainPhotoId");

                    b.HasOne("Bonsai.Data.Models.TreeLayout", "TreeLayout")
                        .WithMany()
                        .HasForeignKey("TreeLayoutId");

                    b.Navigation("MainPhoto");

                    b.Navigation("TreeLayout");
                });

            modelBuilder.Entity("Bonsai.Data.Models.PageAlias", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Page", "Page")
                        .WithMany("Aliases")
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Page");
                });

            modelBuilder.Entity("Bonsai.Data.Models.PageDraft", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Bonsai.Data.Models.PageReference", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Page", "Destination")
                        .WithMany("References")
                        .HasForeignKey("DestinationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Bonsai.Data.Models.Page", "Source")
                        .WithMany()
                        .HasForeignKey("SourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Destination");

                    b.Navigation("Source");
                });

            modelBuilder.Entity("Bonsai.Data.Models.PageScored", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Media", "MainPhoto")
                        .WithMany()
                        .HasForeignKey("MainPhotoId");

                    b.Navigation("MainPhoto");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Relation", b =>
                {
                    b.HasOne("Bonsai.Data.Models.Page", "Destination")
                        .WithMany()
                        .HasForeignKey("DestinationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Bonsai.Data.Models.Page", "Event")
                        .WithMany()
                        .HasForeignKey("EventId");

                    b.HasOne("Bonsai.Data.Models.Page", "Source")
                        .WithMany("Relations")
                        .HasForeignKey("SourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Destination");

                    b.Navigation("Event");

                    b.Navigation("Source");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Bonsai.Data.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Bonsai.Data.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Bonsai.Data.Models.AppUser", b =>
                {
                    b.Navigation("Changes");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Media", b =>
                {
                    b.Navigation("Tags");
                });

            modelBuilder.Entity("Bonsai.Data.Models.Page", b =>
                {
                    b.Navigation("Aliases");

                    b.Navigation("LivingBeingOverview");

                    b.Navigation("MediaTags");

                    b.Navigation("References");

                    b.Navigation("Relations");
                });
#pragma warning restore 612, 618
        }
    }
}

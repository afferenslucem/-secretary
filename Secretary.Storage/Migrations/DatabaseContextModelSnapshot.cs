﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Secretary.Storage.Models;

#nullable disable

namespace Secretary.Storage.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Secretary.Storage.Models.Document", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("DocumentName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<long>("UserChatId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserChatId");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("Secretary.Storage.Models.Email", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("DisplayName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<long>("DocumentId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("DocumentId");

                    b.ToTable("Emails");
                });

            modelBuilder.Entity("Secretary.Storage.Models.EventLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("UserChatId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserChatId");

                    b.ToTable("EventLogs");
                });

            modelBuilder.Entity("Secretary.Storage.Models.User", b =>
                {
                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<string>("AccessToken")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Email")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("JobTitle")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("JobTitleGenitive")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NameGenitive")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("RefreshToken")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTime?>("TokenCreationTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("TokenExpirationSeconds")
                        .HasColumnType("integer");

                    b.HasKey("ChatId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Secretary.Storage.Models.Document", b =>
                {
                    b.HasOne("Secretary.Storage.Models.User", "User")
                        .WithMany("Documents")
                        .HasForeignKey("UserChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Secretary.Storage.Models.Email", b =>
                {
                    b.HasOne("Secretary.Storage.Models.Document", "Document")
                        .WithMany("Emails")
                        .HasForeignKey("DocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Document");
                });

            modelBuilder.Entity("Secretary.Storage.Models.EventLog", b =>
                {
                    b.HasOne("Secretary.Storage.Models.User", "User")
                        .WithMany("Events")
                        .HasForeignKey("UserChatId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Secretary.Storage.Models.Document", b =>
                {
                    b.Navigation("Emails");
                });

            modelBuilder.Entity("Secretary.Storage.Models.User", b =>
                {
                    b.Navigation("Documents");

                    b.Navigation("Events");
                });
#pragma warning restore 612, 618
        }
    }
}

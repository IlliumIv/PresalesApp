﻿// <auto-generated />
using System;
using PresalesMonitor.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PresalesMonitor.Entities.Migrations
{
    [DbContext(typeof(DbController.Context))]
    [Migration("20221110115704_TotalRework")]
    partial class TotalRework
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Entities.Invoice", b =>
                {
                    b.Property<int>("InvoiceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("InvoiceId"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<string>("Counterpart")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("LastPayAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("LastShipmentAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("PresaleId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Profit")
                        .HasColumnType("numeric");

                    b.Property<int?>("ProjectId")
                        .HasColumnType("integer");

                    b.HasKey("InvoiceId");

                    b.HasIndex("PresaleId");

                    b.HasIndex("ProjectId");

                    b.ToTable("Invoices");
                });

            modelBuilder.Entity("Entities.Presale", b =>
                {
                    b.Property<int>("PresaleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PresaleId"));

                    b.Property<int>("Department")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Position")
                        .HasColumnType("integer");

                    b.HasKey("PresaleId");

                    b.ToTable("Presales");
                });

            modelBuilder.Entity("Entities.PresaleAction", b =>
                {
                    b.Property<int>("PresaleActionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PresaleActionId"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<int>("TimeSpend")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("PresaleActionId");

                    b.HasIndex("ProjectId");

                    b.ToTable("Actions");
                });

            modelBuilder.Entity("Entities.ProfitDelta", b =>
                {
                    b.Property<int>("ProfitDeltaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ProfitDeltaId"));

                    b.Property<int>("InvoiceId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("Value")
                        .HasColumnType("numeric");

                    b.HasKey("ProfitDeltaId");

                    b.HasIndex("InvoiceId");

                    b.ToTable("ProfitDeltas");
                });

            modelBuilder.Entity("Entities.Project", b =>
                {
                    b.Property<int>("ProjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ProjectId"));

                    b.Property<DateTime>("ApprovalBySalesDirectorAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("ApprovalByTechDirectorAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("ClosedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LossReason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("MainProjectProjectId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("PotentialAmount")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("PotentialWinAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("PresaleId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("PresaleStartAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("ProjectId");

                    b.HasIndex("MainProjectProjectId");

                    b.HasIndex("PresaleId");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("Entities.Invoice", b =>
                {
                    b.HasOne("Entities.Presale", "Presale")
                        .WithMany("Invoices")
                        .HasForeignKey("PresaleId");

                    b.HasOne("Entities.Project", "Project")
                        .WithMany("Invoices")
                        .HasForeignKey("ProjectId");

                    b.Navigation("Presale");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Entities.PresaleAction", b =>
                {
                    b.HasOne("Entities.Project", "Project")
                        .WithMany("Actions")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Entities.ProfitDelta", b =>
                {
                    b.HasOne("Entities.Invoice", null)
                        .WithMany("ProfitDeltas")
                        .HasForeignKey("InvoiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Entities.Project", b =>
                {
                    b.HasOne("Entities.Project", "MainProject")
                        .WithMany()
                        .HasForeignKey("MainProjectProjectId");

                    b.HasOne("Entities.Presale", "Presale")
                        .WithMany("Projects")
                        .HasForeignKey("PresaleId");

                    b.Navigation("MainProject");

                    b.Navigation("Presale");
                });

            modelBuilder.Entity("Entities.Invoice", b =>
                {
                    b.Navigation("ProfitDeltas");
                });

            modelBuilder.Entity("Entities.Presale", b =>
                {
                    b.Navigation("Invoices");

                    b.Navigation("Projects");
                });

            modelBuilder.Entity("Entities.Project", b =>
                {
                    b.Navigation("Actions");

                    b.Navigation("Invoices");
                });
#pragma warning restore 612, 618
        }
    }
}

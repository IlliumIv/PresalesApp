﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresalesApp.Database.Migrations
{
    [DbContext(typeof(MigrationsEntryPoint.MigrationsContext))]
    [Migration("20221003102758_UpdateProject")]
    partial class UpdateProject
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PresalesStatistic.Entities.Invoice", b =>
                {
                    b.Property<int>("InvoiceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("InvoiceId"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<string>("Counterpart")
                        .HasColumnType("text");

                    b.Property<DateTime>("Data")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastPay")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastShipment")
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

            modelBuilder.Entity("PresalesStatistic.Entities.Presale", b =>
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

            modelBuilder.Entity("PresalesStatistic.Entities.PresaleAction", b =>
                {
                    b.Property<int>("PresaleActionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PresaleActionId"));

                    b.Property<DateTime?>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int?>("Number")
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

            modelBuilder.Entity("PresalesStatistic.Entities.Project", b =>
                {
                    b.Property<int>("ProjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ProjectId"));

                    b.Property<DateTime?>("ApprovalBySalesDirector")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ApprovalByTechDirector")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LossReason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("MainProjectProjectId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("PotentialAmount")
                        .HasColumnType("numeric");

                    b.Property<int?>("PresaleId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("PresaleStart")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("ProjectId");

                    b.HasIndex("MainProjectProjectId");

                    b.HasIndex("PresaleId");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("PresalesStatistic.Entities.Invoice", b =>
                {
                    b.HasOne("PresalesStatistic.Entities.Presale", "Presale")
                        .WithMany("Invoices")
                        .HasForeignKey("PresaleId");

                    b.HasOne("PresalesStatistic.Entities.Project", "Project")
                        .WithMany("Invoices")
                        .HasForeignKey("ProjectId");

                    b.Navigation("Presale");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("PresalesStatistic.Entities.PresaleAction", b =>
                {
                    b.HasOne("PresalesStatistic.Entities.Project", "Project")
                        .WithMany("Actions")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("PresalesStatistic.Entities.Project", b =>
                {
                    b.HasOne("PresalesStatistic.Entities.Project", "MainProject")
                        .WithMany()
                        .HasForeignKey("MainProjectProjectId");

                    b.HasOne("PresalesStatistic.Entities.Presale", "Presale")
                        .WithMany("Projects")
                        .HasForeignKey("PresaleId");

                    b.Navigation("MainProject");

                    b.Navigation("Presale");
                });

            modelBuilder.Entity("PresalesStatistic.Entities.Presale", b =>
                {
                    b.Navigation("Invoices");

                    b.Navigation("Projects");
                });

            modelBuilder.Entity("PresalesStatistic.Entities.Project", b =>
                {
                    b.Navigation("Actions");

                    b.Navigation("Invoices");
                });
#pragma warning restore 612, 618
        }
    }
}

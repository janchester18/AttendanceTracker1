﻿// <auto-generated />
using System;
using AttendanceTracker1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AttendanceTracker1.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AttendanceTracker1.Models.Attendance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("BreakFinish")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("BreakStart")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ClockIn")
                        .HasColumnType("datetime2");

                    b.Property<double?>("ClockInLatitude")
                        .HasColumnType("float");

                    b.Property<double?>("ClockInLongitude")
                        .HasColumnType("float");

                    b.Property<DateTime?>("ClockOut")
                        .HasColumnType("datetime2");

                    b.Property<double?>("ClockOutLatitude")
                        .HasColumnType("float");

                    b.Property<double?>("ClockOutLongitude")
                        .HasColumnType("float");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Remarks")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Attendances");
                });

            modelBuilder.Entity("AttendanceTracker1.Models.Leave", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Reason")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("RejectionReason")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int?>("ReviewedBy")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ReviewedBy");

                    b.HasIndex("UserId");

                    b.ToTable("Leaves");
                });

            modelBuilder.Entity("AttendanceTracker1.Models.Overtime", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan>("EndTime")
                        .HasColumnType("time");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("RejectionReason")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int?>("ReviewedBy")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("time");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ReviewedBy");

                    b.HasIndex("UserId");

                    b.ToTable("Overtimes");
                });

            modelBuilder.Entity("AttendanceTracker1.Models.OvertimeConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<double>("BreaktimeMax")
                        .HasColumnType("float");

                    b.Property<TimeSpan>("OfficeEndTime")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("OfficeStartTime")
                        .HasColumnType("time");

                    b.Property<double>("OvertimeDailyMax")
                        .HasColumnType("float");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("OvertimeConfigs");
                });

            modelBuilder.Entity("AttendanceTracker1.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("AttendanceTracker1.Models.Attendance", b =>
                {
                    b.HasOne("AttendanceTracker1.Models.User", "User")
                        .WithMany("Attendances")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("AttendanceTracker1.Models.Leave", b =>
                {
                    b.HasOne("AttendanceTracker1.Models.User", "Approver")
                        .WithMany("Approvals")
                        .HasForeignKey("ReviewedBy")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("AttendanceTracker1.Models.User", "User")
                        .WithMany("Leaves")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Approver");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AttendanceTracker1.Models.Overtime", b =>
                {
                    b.HasOne("AttendanceTracker1.Models.User", "Approver")
                        .WithMany("OvertimeApprovals")
                        .HasForeignKey("ReviewedBy")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("AttendanceTracker1.Models.User", "User")
                        .WithMany("OvertimeRequests")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Approver");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AttendanceTracker1.Models.User", b =>
                {
                    b.Navigation("Approvals");

                    b.Navigation("Attendances");

                    b.Navigation("Leaves");

                    b.Navigation("OvertimeApprovals");

                    b.Navigation("OvertimeRequests");
                });
#pragma warning restore 612, 618
        }
    }
}

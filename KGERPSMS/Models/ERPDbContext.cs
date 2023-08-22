using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace SmsSchedulerCore.Models
{
    public partial class ERPDbContext : DbContext
    {
        public ERPDbContext()
        {
        }

        public ERPDbContext(DbContextOptions<ERPDbContext> options)
            : base(options)
        {
        }
        
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<ErpSm> ErpSms { get; set; }
        public virtual DbSet<SmsType> SmsTypes { get; set; }
        public virtual DbSet<VwSmslist> VwSmslists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);
            var x = builder.Build();
            if (!optionsBuilder.IsConfigured)
            { 
                optionsBuilder.UseSqlServer(x.GetConnectionString("KGERPDB"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Company");

                entity.Property(e => e.CompanyId).ValueGeneratedNever();

                entity.Property(e => e.Action)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Address)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyLogo)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Controller)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Fax)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.MushokNo).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Param)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ShortName)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Company_Company");
            });

            modelBuilder.Entity<ErpSm>(entity =>
            {
                entity.ToTable("ErpSMS");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Message).IsRequired();

                entity.Property(e => e.PhoneNo)
                    .IsRequired()
                    .HasMaxLength(11);

                entity.Property(e => e.RowTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.ErpSms)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ErpSMS_Company");

                entity.HasOne(d => d.SmsTypeNavigation)
                    .WithMany(p => p.ErpSms)
                    .HasForeignKey(d => d.SmsType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ErpSMS_SmsType");
            });

            modelBuilder.Entity<SmsType>(entity =>
            {
                entity.ToTable("SmsType");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<VwSmslist>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vwSMSList");

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Message).IsRequired();

                entity.Property(e => e.PhoneNo)
                    .IsRequired()
                    .HasMaxLength(11);

                entity.Property(e => e.RowTime).HasColumnType("datetime");

                entity.Property(e => e.SmstypeName)
                    .IsRequired()
                    .HasColumnName("SMSTypeName");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

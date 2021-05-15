using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BeachHouseAPI.Models
{
    public partial class BeachHouseContext : DbContext
    {
        public BeachHouseContext()
        {
        }

        public BeachHouseContext(DbContextOptions<BeachHouseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Seasons> Seasons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=CRS-LAP-030;Database=BeachHouse;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Seasons>(entity =>
            {
                entity.Property(e => e.Active).HasColumnName("ACTIVE");

                entity.Property(e => e.DescriptionSeason)
                    .IsRequired()
                    .HasColumnName("descriptionSeason")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Enddate)
                    .HasColumnName("enddate")
                    .HasColumnType("date");

                entity.Property(e => e.Startdate)
                    .HasColumnName("startdate")
                    .HasColumnType("date");

                entity.Property(e => e.Typeseason).HasColumnName("typeseason");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

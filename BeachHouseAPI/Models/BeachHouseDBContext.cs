using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BeachHouseAPI.Models
{
    public partial class BeachHouseDBContext : DbContext
    {
        public BeachHouseDBContext()
        {
        }

        public BeachHouseDBContext(DbContextOptions<BeachHouseDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Locations> Locations { get; set; }
        public virtual DbSet<Logs> Logs { get; set; }
        public virtual DbSet<Params> Params { get; set; }
        public virtual DbSet<ReservationDetails> ReservationDetails { get; set; }
        public virtual DbSet<Reservations> Reservations { get; set; }
        public virtual DbSet<ReservationLog> ReservationLog { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Seasons> Seasons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlServer("Server=CRH-LAP-106\\SQLEXPRESS;Database=BeachHouseDB;Trusted_Connection=True;");
                optionsBuilder.UseSqlServer("Server=tcp:beachhouse-test.database.windows.net,1433;Initial Catalog=beachhouse;Persist Security Info=False;User ID=beachhouseadmin;Password=Mjcg7982;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Locations>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Logs>(entity =>
            {
                entity.Property(e => e.Level).HasMaxLength(128);

                entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            });

            modelBuilder.Entity<Params>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.LastModified)
                    .HasColumnName("last_modified")
                    .HasColumnType("datetime");

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasColumnName("updated_by")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.UpdatedByNavigation)
                    .WithMany(p => p.Params)
                    .HasForeignKey(d => d.UpdatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Params_ToUser");
            });

            modelBuilder.Entity<ReservationDetails>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.ReservationId });

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ReservationId).HasColumnName("reservation_id");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("date");

                entity.Property(e => e.Rate).HasColumnName("rate");

                entity.HasOne(d => d.Reservation)
                    .WithMany(p => p.ReservationDetails)
                    .HasForeignKey(d => d.ReservationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReservationDetails_ToMaster");
            });

            modelBuilder.Entity<Reservations>(entity =>
            {
                entity.Property(e => e.Active).HasColumnName("active");
                entity.Property(e => e.Notified).HasColumnName("notified");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("datetime");

                entity.Property(e => e.LocationId).HasColumnName("location_id");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("user_id")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Reservations)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Location_ToReservation");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Reservations)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Users_ToReservation");
            });

            modelBuilder.Entity<ReservationLog>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Operation)
                    .HasColumnName("operation")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReservationId).HasColumnName("reservation_id");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("user_id")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Reservation)
                    .WithMany(p => p.ReservationLog)
                    .HasForeignKey(d => d.ReservationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Log_ToReservation");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ReservationLog)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_ToReservationLog");
            });


            modelBuilder.Entity<Users>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Email).HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Phone).HasColumnName("phone");

                entity.Property(e => e.Role).HasColumnName("role");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

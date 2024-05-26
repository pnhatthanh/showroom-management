using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WebPBL3.Models;
using WebPBL3.DTO.Staff;

namespace WebPBL3.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Fluent API
            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasOne(c => c.Make)
                .WithMany(m => m.Cars)
                .HasForeignKey("MakeID")
                .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<District>(entity =>
            {
                entity.HasOne(d => d.Province)
                .WithMany(p => p.Districts)
                .HasForeignKey("ProvinceID")
                .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<Ward>(entity =>
            {
                entity.HasOne(w => w.District)
                .WithMany(d => d.Wards)
                .HasForeignKey("DistrictID")
                .OnDelete(DeleteBehavior.NoAction);
            });
            
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasOne(a => a.Role)
                .WithMany()
                .HasForeignKey(a => a.RoleID)
                .OnDelete(DeleteBehavior.NoAction);

                
            });
            modelBuilder.Entity<User>(entity =>
            {


                entity.HasOne(u => u.Ward)
                .WithMany()
                .HasForeignKey(u => u.WardID)
                .OnDelete(DeleteBehavior.NoAction);
                entity.HasMany(f => f.Feedbacks)
                .WithOne(u => u.User)
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.NoAction);

                
                entity.HasOne(u => u.Account)
               .WithOne(a => a.User)
               .HasForeignKey<User>(u => u.AccountID)
               .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Staff>(entity =>
            {
                
                entity.HasMany(n => n.News)
                .WithOne(s => s.Staff)
                .HasForeignKey(s => s.StaffID)
                .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(a => a.User)
                .WithOne(u => u.Staff)
                .HasForeignKey<Staff>(s => s.UserID)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>(entity =>
            {
            entity.HasOne(u => u.User)
            .WithMany(o => o.Orders)
            .HasForeignKey("UserID")
            .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(s => s.Staff)
            .WithMany()
            .HasForeignKey("StaffID")
            .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<DetailOrder>(entity =>
            {
                entity.HasOne(c => c.Car)
                .WithMany()
                .HasForeignKey("CarID")
                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(o => o.Order)
                .WithMany(dto => dto.DetailOrders)
                .HasForeignKey("OrderID")
                .OnDelete(DeleteBehavior.NoAction);
            });




        }

        // Định nghĩa các DbSet cho các bảng trong cơ sở dữ liệu
        // Ví dụ:
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DetailOrder> DetailOrders { get; set; }
        public DbSet<News> NewS { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Make> Makes { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        





    }
}
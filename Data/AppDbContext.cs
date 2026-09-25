using Microsoft.EntityFrameworkCore;
using Reservation_Que.Models;

namespace Reservation_Que.Data
{
    /// <summary>
    /// EF Core DbContext ศูนย์กลางการเข้าถึงฐานข้อมูล
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Queue> Queues => Set<Queue>();
        public DbSet<TableSetting> TableSettings => Set<TableSetting>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // เก็บ enum Status เป็นตัวเลข (int) ในฐานข้อมูล
            modelBuilder.Entity<Queue>()
                .Property(q => q.Status)
                .HasConversion<int>();

            // กลุ่มโต๊ะต้องไม่ซ้ำกัน
            modelBuilder.Entity<TableSetting>()
                .HasIndex(t => t.TableGroup)
                .IsUnique();

            // ค่าเริ่มต้นของแต่ละกลุ่มโต๊ะ (เวลานั่งสูงสุด 90 นาที = 1.30 ชม.)
            modelBuilder.Entity<TableSetting>().HasData(
                new TableSetting { Id = 1, TableGroup = "A", Description = "1-2 คน", MaxMinutes = 90 },
                new TableSetting { Id = 2, TableGroup = "B", Description = "3-4 คน", MaxMinutes = 90 },
                new TableSetting { Id = 3, TableGroup = "C", Description = "5 คนขึ้นไป", MaxMinutes = 90 }
            );
        }
    }
}

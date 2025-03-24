using Microsoft.EntityFrameworkCore;
using testAPI.Models;

namespace testAPI.Data
{
    public class PowerDbContext : DbContext
    {
        public PowerDbContext(DbContextOptions<PowerDbContext> options) : base(options)
        {
        }

        public DbSet<MeterInfo> MeterInfo { get; set; }
        public DbSet<PMMinP> PMMinP { get; set; }
        public DbSet<LightInfo> LightInfo { get; set; }
        public DbSet<LightControl> LightControl { get; set; }
        public DbSet<ACCommand> ACCommand { get; set; }
        public DbSet<ACControl> ACControl { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 設定對應到 SQL Server 資料表名稱
            modelBuilder.Entity<MeterInfo>().ToTable("Meter_Info");
            modelBuilder.Entity<PMMinP>().ToTable("PM_Min_P");
            modelBuilder.Entity<LightInfo>().ToTable("Light_Info");
            modelBuilder.Entity<LightControl>().ToTable("Light_Control");
            modelBuilder.Entity<ACCommand>().ToTable("AC_Command");
            modelBuilder.Entity<ACControl>().ToTable("AC_Control");

            // 設定複合主鍵
            modelBuilder.Entity<ACCommand>()
                .HasKey(ac => new { ac.AC_Id, ac.Ch });

            modelBuilder.Entity<ACControl>()
                .HasKey(ac => new { ac.Date_Time, ac.R_Id, ac.Ch, ac.Com_Id });

            modelBuilder.Entity<LightControl>()
                .HasKey(lc => new { lc.COM_Id, lc.ICP_Id, lc.Date_Time });

            modelBuilder.Entity<LightInfo>()
                .HasKey(li => new { li.COM_Id, li.ICP_Id });

            modelBuilder.Entity<PMMinP>()
                .HasKey(pm => new { pm.Date_time, pm.Meter_Id });
        }
    }
}

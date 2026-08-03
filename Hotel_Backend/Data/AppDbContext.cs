using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Models;

namespace Hotel_Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<TableCategory> TableCategories { get; set; }
        public DbSet<RestaurantTable> RestaurantTables { get; set; }
        public DbSet<TableMergeGroup> TableMergeGroups { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ParcelCodeSequence> ParcelCodeSequences { get; set; }
        public DbSet<RestaurantBill> RestaurantBills { get; set; }
        public DbSet<BillSplit> BillSplits { get; set; }
        public DbSet<HotelSetting> HotelSettings { get; set; }
        public DbSet<WaitlistEntry> WaitlistEntries { get; set; }
        public DbSet<RoomCategory> RoomCategories { get; set; }
        public DbSet<SeasonalRule> SeasonalRules { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<RoomBill> RoomBills { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Chef> Chefs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique Client Email
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Configure unique Category Name
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Configure unique TableCategory Name
            modelBuilder.Entity<TableCategory>()
                .HasIndex(tc => tc.Name)
                .IsUnique();

            // Configure Category to MenuItems relationship
            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure RestaurantTable unique constraints
            modelBuilder.Entity<RestaurantTable>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<RestaurantTable>()
                .HasIndex(t => t.QrToken)
                .IsUnique();

            // Configure RestaurantTable to TableCategory relationship
            modelBuilder.Entity<RestaurantTable>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tables)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure RestaurantTable to TableMergeGroup relationship
            modelBuilder.Entity<RestaurantTable>()
                .HasOne(t => t.MergeGroup)
                .WithMany(g => g.Tables)
                .HasForeignKey(t => t.MergeGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure RestaurantBill
            modelBuilder.Entity<RestaurantBill>()
                .HasIndex(b => b.BillNumber)
                .IsUnique();

            modelBuilder.Entity<RestaurantBill>()
                .HasOne(b => b.Order)
                .WithMany()
                .HasForeignKey(b => b.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BillSplit>()
                .HasOne(s => s.RestaurantBill)
                .WithMany(b => b.Splits)
                .HasForeignKey(s => s.RestaurantBillId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure WaitlistEntry to RestaurantTable relationship
            modelBuilder.Entity<WaitlistEntry>()
                .HasOne(w => w.AssignedTable)
                .WithMany()
                .HasForeignKey(w => w.AssignedTableId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure RoomCategory unique name
            modelBuilder.Entity<RoomCategory>()
                .HasIndex(rc => rc.Name)
                .IsUnique();

            // Configure SeasonalRule to RoomCategory relationship
            modelBuilder.Entity<SeasonalRule>()
                .HasOne(sr => sr.RoomCategory)
                .WithMany(rc => rc.SeasonalRules)
                .HasForeignKey(sr => sr.RoomCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Room unique number
            modelBuilder.Entity<Room>()
                .HasIndex(r => r.Number)
                .IsUnique();
        }
    }
}

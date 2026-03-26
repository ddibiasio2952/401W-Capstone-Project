using FalveyInsuranceGroup.Backend.Controllers;
using FalveyInsuranceGroup.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace FalveyInsuranceGroup.Db
{
    // Allows us to interact with our database
    public class FalveyInsuranceGroupContext : DbContext
    {
        public FalveyInsuranceGroupContext(DbContextOptions<FalveyInsuranceGroupContext> options):base(options) 
        { 
        }

        // Override method from the base DbContext class
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Announcement>();
            modelBuilder.Entity<Claim>();
            modelBuilder.Entity<ClaimNote>();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerRecord>()
                .ToTable("customer_records");
            modelBuilder.Entity<Employee>()
                .HasIndex(employee_index => employee_index.email)
                .IsUnique();
            modelBuilder.Entity<Location>();
            modelBuilder.Entity<LoginAudit>();
            modelBuilder.Entity<Policy>();
            modelBuilder.Entity<Release>();
            modelBuilder.Entity<Session>()
                .HasIndex(session_index => session_index.session_id)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(user_index => user_index.email)
                .IsUnique();

        }

        // Specify DbSet instance for operating with databases
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimNote> ClaimNotes { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerRecord> CustomerRecords { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<LoginAudit> LoginAudits { get; set; }
        public DbSet<Memo> Memos { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<Release> Releases { get; set; }
        public DbSet<Session> Sessions { get; set; }
         public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<User> Users { get; set; }
    }
}

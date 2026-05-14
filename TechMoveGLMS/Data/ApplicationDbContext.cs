using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Models;

namespace TechMoveGLMS.Data
{
    // This class connects our C# models to the SQL database
    public class ApplicationDbContext : DbContext
    {
        // Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // These are our database tables
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        // This method adds some example data to the database
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Adding sample clients
            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    ClientId = 1,
                    Name = "ABC Logistics",
                    Email = "contact@abclogistics.co.za",
                    Phone = "0123456789",
                    Region = "Gauteng"
                },
                new Client
                {
                    ClientId = 2,
                    Name = "FastShip SA",
                    Email = "info@fastship.co.za",
                    Phone = "0112345678",
                    Region = "Western Cape"
                },
                new Client
                {
                    ClientId = 3,
                    Name = "Cargo Masters",
                    Email = "support@cargomasters.co.za",
                    Phone = "0317654321",
                    Region = "KwaZulu-Natal"
                }
            );

            // Adding sample contracts
            modelBuilder.Entity<Contract>().HasData(
                new Contract
                {
                    ContractId = 1,
                    ClientId = 1,
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2026, 1, 1),
                    Status = "Active",
                    ServiceLevel = "Premium"
                },
                new Contract
                {
                    ContractId = 2,
                    ClientId = 2,
                    StartDate = new DateTime(2025, 3, 1),
                    EndDate = new DateTime(2025, 12, 31),
                    Status = "Draft",
                    ServiceLevel = "Standard"
                },
                new Contract
                {
                    ContractId = 3,
                    ClientId = 1,
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2024, 12, 31),
                    Status = "Expired",
                    ServiceLevel = "Basic"
                }
            );
        }
    }
}
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Models;

namespace TechMoveGLMS.Repositories
{
    // This class DOES the actual database work
    // It implements (fulfills the promise of) IContractRepository
    public class ContractRepository : IContractRepository
    {
        // This is your database connection (same one you used in your controller)
        private readonly ApplicationDbContext _context;

        // Constructor - receives the database context when created
        // STUDENT NOTE: This is Dependency Injection - the system gives us what we need
        public ContractRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get ALL contracts, AND include their related Client data
        // STUDENT NOTE: Include() is like SQL JOIN - gets parent data too
        public async Task<List<Contract>> GetAllContractsAsync()
        {
            return await _context.Contracts
                .Include(c => c.Client)  // Bring the client info along
                .ToListAsync();          // Execute the query and get list
        }

        // Get ONE contract by ID - simple, no extra data
        public async Task<Contract?> GetContractByIdAsync(int id)
        {
            return await _context.Contracts.FindAsync(id);  // FindAsync is fast for primary keys
        }

        // Get ONE contract WITH its related data (Client AND ServiceRequests)
        public async Task<Contract?> GetContractWithDetailsAsync(int id)
        {
            return await _context.Contracts
                .Include(c => c.Client)           // Get the client info
                .Include(c => c.ServiceRequests)  // Get all service requests for this contract
                .FirstOrDefaultAsync(m => m.ContractId == id);  // Find matching contract
        }

        // Add a new contract to the database (but don't save yet)
        public async Task AddContractAsync(Contract contract)
        {
            await _context.Contracts.AddAsync(contract);  // Queues the insert operation
        }

        // Mark a contract as modified (for updates)
        public async Task UpdateContractAsync(Contract contract)
        {
            _context.Contracts.Update(contract);  // Queues the update operation
            await Task.CompletedTask;  // Nothing async actually happens - this just satisfies the method signature
        }

        // Check if a contract exists to avoid "not found" errors
        public async Task<bool> ContractExistsAsync(int id)
        {
            return await _context.Contracts.AnyAsync(e => e.ContractId == id);
        }

        // ACTUALLY save everything to the database
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();  // This is where the INSERT/UPDATE really happens
        }
    }
}
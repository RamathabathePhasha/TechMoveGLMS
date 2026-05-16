using TechMoveGLMS.Models;

namespace TechMoveGLMS.Repositories
{
    // This is like a promise: "Any class that implements me MUST have these methods"
    public interface IContractRepository
    {
        // These are just method signatures (blueprints)
        // The actual code goes in ContractRepository.cs

        // Get all contracts from database
        Task<List<Contract>> GetAllContractsAsync();

        // Get ONE contract by its ID (without extra data)
        Task<Contract?> GetContractByIdAsync(int id);

        // Get ONE contract WITH its related data (Client, ServiceRequests)
        Task<Contract?> GetContractWithDetailsAsync(int id);

        // Add a new contract to database
        Task AddContractAsync(Contract contract);

        // Update an existing contract
        Task UpdateContractAsync(Contract contract);

        // Check if a contract exists (used to avoid errors)
        Task<bool> ContractExistsAsync(int id);

        // Save all changes to database (like clicking "Save" in Word)
        Task SaveChangesAsync();
    }
}
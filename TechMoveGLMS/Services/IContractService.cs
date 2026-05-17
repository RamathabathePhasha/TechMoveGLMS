using TechMoveGLMS.Models;

namespace TechMoveGLMS.Services
{
    // This defines what our business logic layer can do
    public interface IContractService
    {
        // Get all contracts (for the Index page)
        Task<List<Contract>> GetAllContractsAsync();

        // Get contract with all details (for Details page)
        Task<Contract?> GetContractDetailsAsync(int id);

        // Get contract for editing (simpler - doesn't need related data)
        Task<Contract?> GetContractForEditAsync(int id);

        // Create a new contract with file upload
        //The tuple (bool, string?) returns either success flag OR error message
        Task<(bool Success, string? Error)> CreateContractAsync(Contract contract, string? filePath, string? fileName);

        // Update existing contract
        Task<(bool Success, string? Error)> UpdateContractAsync(Contract contract, IFormFile? newFile, string? oldFilePath, string? oldFileName, IWebHostEnvironment webHostEnvironment);

        // Search/filter contracts
        Task<List<Contract>> SearchContractsAsync(DateTime? startDate, DateTime? endDate, string? status);

        // Delete a file when contract is updated or deleted
        Task DeleteOldFileAsync(string? filePath, IWebHostEnvironment webHostEnvironment);
    }
}
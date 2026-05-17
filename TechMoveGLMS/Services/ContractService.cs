using TechMoveGLMS.Models;
using TechMoveGLMS.Repositories;

namespace TechMoveGLMS.Services
{
    // This class contains ALL your business rules
    // It USES the repository to save/load data, but doesn't know HOW the repository works
    public class ContractService : IContractService
    {
        // The service uses the repository (dependency)
        private readonly IContractRepository _contractRepository;

        // Constructor receives the repository (Dependency Injection again)
        public ContractService(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        //Simple pass-through - service just asks repository for data
        public async Task<List<Contract>> GetAllContractsAsync()
        {
            return await _contractRepository.GetAllContractsAsync();
        }

        // Get detailed contract info (with related data)
        public async Task<Contract?> GetContractDetailsAsync(int id)
        {
            return await _contractRepository.GetContractWithDetailsAsync(id);
        }

        // Get contract without related data (faster for editing)
        public async Task<Contract?> GetContractForEditAsync(int id)
        {
            return await _contractRepository.GetContractByIdAsync(id);
        }

        //This is where business rules LIVE
        // Create a new contract
        public async Task<(bool Success, string? Error)> CreateContractAsync(Contract contract, string? filePath, string? fileName)
        {
            // Set the file information
            contract.SignedAgreementPath = filePath;
            contract.SignedAgreementFileName = fileName;

            // Add to repository (just queues it)
            await _contractRepository.AddContractAsync(contract);

            // ACTUALLY save to database
            await _contractRepository.SaveChangesAsync();

            return (true, null);  // Success!
        }

        // Update an existing contract
        public async Task<(bool Success, string? Error)> UpdateContractAsync(
            Contract contract,
            IFormFile? newFile,
            string? oldFilePath,
            string? oldFileName,
            IWebHostEnvironment webHostEnvironment)
        {
            //If no new file was uploaded, keep the old one
            if (newFile == null)
            {
                contract.SignedAgreementPath = oldFilePath;
                contract.SignedAgreementFileName = oldFileName;
            }
            // If there IS a new file, the controller already saved it and updated the paths

            // Mark contract as modified
            await _contractRepository.UpdateContractAsync(contract);

            // Save changes to database
            await _contractRepository.SaveChangesAsync();

            return (true, null);
        }

        //Search/filter logic using LINQ
        // This is business logic - defining HOW to filter
        public async Task<List<Contract>> SearchContractsAsync(DateTime? startDate, DateTime? endDate, string? status)
        {
            // First, get ALL contracts from repository
            var allContracts = await _contractRepository.GetAllContractsAsync();

            // Now apply filters (business rules for searching)
            var query = allContracts.AsQueryable();

            // Rule 1: Filter by start date if provided
            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

            // Rule 2: Filter by end date if provided
            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value);
            }

            // Rule 3: Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }

            // Return filtered results
            return query.ToList();
        }

        //Helper method to delete old files from server
        // This prevents cluttering the server with old PDFs
        public async Task DeleteOldFileAsync(string? filePath, IWebHostEnvironment webHostEnvironment)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                // Convert web path to physical file path
                string fullPath = Path.Combine(webHostEnvironment.WebRootPath, filePath.TrimStart('/'));

                // Delete if it exists
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            await Task.CompletedTask;  // Satisfies async requirement
        }
    }
}
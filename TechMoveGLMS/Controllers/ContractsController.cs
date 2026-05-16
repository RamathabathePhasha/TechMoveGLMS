using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS.Models;
using TechMoveGLMS.Services;  // We need this to use IContractService

namespace TechMoveGLMS.Controllers
{
    // STUDENT NOTE: This controller is now THIN - it does very little work
    // All the real work is delegated to the Service layer
    public class ContractsController : Controller
    {
        // The controller ONLY knows about the Service (not the database!)
        private readonly IContractService _contractService;
        private readonly IWebHostEnvironment _webHostEnvironment;  // Still needed for file paths

        // Constructor receives the service via Dependency Injection
        public ContractsController(IContractService contractService, IWebHostEnvironment webHostEnvironment)
        {
            _contractService = contractService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Contracts - Show all contracts
        // STUDENT NOTE: Controller just asks service for data, then shows view
        public async Task<IActionResult> Index()
        {
            var contracts = await _contractService.GetAllContractsAsync();
            return View(contracts);
        }

        // GET: Contracts/Details/5 - Show one contract with details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _contractService.GetContractDetailsAsync(id.Value);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contracts/Create - Show the form
        public IActionResult Create()
        {
            return View();
        }

        // POST: Contracts/Create - Save new contract
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile? signedAgreement)
        {
            if (ModelState.IsValid)
            {
                string? filePath = null;
                string? fileName = null;

                // Handle file upload (this is still in controller because it's HTTP/web concern)
                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    // BUSINESS RULE: Only PDF files allowed
                    var fileExtension = Path.GetExtension(signedAgreement.FileName).ToLower();
                    if (fileExtension != ".pdf")
                    {
                        ModelState.AddModelError("signedAgreement", "ONLY PDF files are allowed!");
                        return View(contract);
                    }

                    // Save the file to disk (web concern)
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + signedAgreement.FileName;
                    string fullFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await signedAgreement.CopyToAsync(fileStream);
                    }

                    filePath = "/uploads/" + uniqueFileName;
                    fileName = signedAgreement.FileName;
                }

                // STUDENT NOTE: Now ask the SERVICE to create the contract
                var result = await _contractService.CreateContractAsync(contract, filePath, fileName);

                if (result.Success)
                {
                    TempData["Success"] = "Contract created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.Error ?? "Error creating contract");
            }

            return View(contract);
        }

        // GET: Contracts/Edit/5 - Show edit form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _contractService.GetContractForEditAsync(id.Value);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contracts/Edit/5 - Save edited contract
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContractId,ClientId,StartDate,EndDate,Status,ServiceLevel,SignedAgreementPath,SignedAgreementFileName")] Contract contract, IFormFile? signedAgreement)
        {
            if (id != contract.ContractId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string? oldFilePath = contract.SignedAgreementPath;
                string? oldFileName = contract.SignedAgreementFileName;

                // Handle new file upload if provided
                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    // BUSINESS RULE: Validate PDF
                    var fileExtension = Path.GetExtension(signedAgreement.FileName).ToLower();
                    if (fileExtension != ".pdf")
                    {
                        ModelState.AddModelError("signedAgreement", "ONLY PDF files are allowed!");
                        return View(contract);
                    }

                    // Delete old file from disk (web concern)
                    if (!string.IsNullOrEmpty(oldFilePath))
                    {
                        string oldFullPath = Path.Combine(_webHostEnvironment.WebRootPath, oldFilePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFullPath))
                        {
                            System.IO.File.Delete(oldFullPath);
                        }
                    }

                    // Save new file
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + signedAgreement.FileName;
                    string fullFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await signedAgreement.CopyToAsync(fileStream);
                    }

                    contract.SignedAgreementPath = "/uploads/" + uniqueFileName;
                    contract.SignedAgreementFileName = signedAgreement.FileName;
                }

                // Ask service to update
                var result = await _contractService.UpdateContractAsync(contract, signedAgreement, oldFilePath, oldFileName, _webHostEnvironment);

                if (result.Success)
                {
                    TempData["Success"] = "Contract updated!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.Error ?? "Error updating contract");
            }

            return View(contract);
        }

        // GET: Contracts/Search - Search/filter contracts
        // STUDENT NOTE: The service handles the filtering logic
        public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate, string? status)
        {
            var results = await _contractService.SearchContractsAsync(startDate, endDate, status);

            // Pass search parameters back to view for display
            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;
            ViewBag.CurrentStatus = status;
            ViewBag.Statuses = new List<string> { "Draft", "Active", "Expired", "OnHold" };

            return View(results);
        }

        // GET: Contracts/DownloadFile/5 - Download PDF
        public async Task<IActionResult> DownloadFile(int id)
        {
            var contract = await _contractService.GetContractDetailsAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
            {
                TempData["Error"] = "No file available to download";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, contract.SignedAgreementPath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "File not found on server";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", contract.SignedAgreementFileName ?? "contract.pdf");
        }
    }
}
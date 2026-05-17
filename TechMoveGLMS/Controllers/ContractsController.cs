using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Models;
using TechMoveGLMS.Services;

namespace TechMoveGLMS.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;  // For dropdown lists

        public ContractsController(IContractService contractService, IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _contractService = contractService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        // GET: Contracts
        public async Task<IActionResult> Index()
        {
            var contracts = await _contractService.GetAllContractsAsync();
            return View(contracts);
        }

        // GET: Contracts/Details/5
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

        // GET: Contracts/Create
        public IActionResult Create()
        {
            // FIX: Load clients for dropdown
            ViewBag.Clients = _context.Clients.ToList();
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile? signedAgreement)
        {
            if (ModelState.IsValid)
            {
                string? filePath = null;
                string? fileName = null;

                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    var fileExtension = Path.GetExtension(signedAgreement.FileName).ToLower();
                    if (fileExtension != ".pdf")
                    {
                        ModelState.AddModelError("signedAgreement", "ONLY PDF files are allowed!");
                        ViewBag.Clients = _context.Clients.ToList();  // Reload on error
                        return View(contract);
                    }

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

                var result = await _contractService.CreateContractAsync(contract, filePath, fileName);

                if (result.Success)
                {
                    TempData["Success"] = "Contract created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.Error ?? "Error creating contract");
            }

            ViewBag.Clients = _context.Clients.ToList();  // Reload on error
            return View(contract);
        }

        // GET: Contracts/Edit/5
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

            // FIX: Load clients for dropdown in Edit view
            ViewBag.Clients = _context.Clients.ToList();

            return View(contract);
        }

        // POST: Contracts/Edit/5
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

                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    var fileExtension = Path.GetExtension(signedAgreement.FileName).ToLower();
                    if (fileExtension != ".pdf")
                    {
                        ModelState.AddModelError("signedAgreement", "ONLY PDF files are allowed!");
                        ViewBag.Clients = _context.Clients.ToList();  // Reload on error
                        return View(contract);
                    }

                    if (!string.IsNullOrEmpty(oldFilePath))
                    {
                        string oldFullPath = Path.Combine(_webHostEnvironment.WebRootPath, oldFilePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFullPath))
                        {
                            System.IO.File.Delete(oldFullPath);
                        }
                    }

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

                var result = await _contractService.UpdateContractAsync(contract, signedAgreement, oldFilePath, oldFileName, _webHostEnvironment);

                if (result.Success)
                {
                    TempData["Success"] = "Contract updated!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.Error ?? "Error updating contract");
            }

            ViewBag.Clients = _context.Clients.ToList();  // Reload on error
            return View(contract);
        }

        // GET: Contracts/Search
        public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate, string? status)
        {
            var results = await _contractService.SearchContractsAsync(startDate, endDate, status);

            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;
            ViewBag.CurrentStatus = status;
            ViewBag.Statuses = new List<string> { "Draft", "Active", "Expired", "OnHold" };

            return View(results);
        }

        // GET: Contracts/DownloadFile/5
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Models;

namespace TechMoveGLMS.Controllers
{
    // This controller handles everything related to Contracts
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // For file handling

        public ContractsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Contracts - Show all contracts
        public async Task<IActionResult> Index()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Client)  // Include the client info
                .ToListAsync();
            return View(contracts);
        }

        // GET: Contracts/Details/5 - Show one contract details
        // GET: Contracts/Details/5
        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(m => m.ContractId == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contracts/Create - Show form to create contract
        public IActionResult Create()
        {
            ViewBag.Clients = _context.Clients.ToList();
            return View();
        }

        // POST: Contracts/Create - Save new contract with PDF upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile? signedAgreement)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload if user selected a file
                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    // VALIDATION: Only PDF files allowed!
                    var fileExtension = Path.GetExtension(signedAgreement.FileName).ToLower();
                    if (fileExtension != ".pdf")
                    {
                        ModelState.AddModelError("signedAgreement", "ONLY PDF files are allowed!");
                        ViewBag.Clients = _context.Clients.ToList();
                        return View(contract);
                    }

                    // Create uploads folder if it doesn't exist
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Create a unique filename (to avoid overwriting)
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + signedAgreement.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file to disk
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await signedAgreement.CopyToAsync(fileStream);
                    }

                    // Save the file path in the database
                    contract.SignedAgreementPath = "/uploads/" + uniqueFileName;
                    contract.SignedAgreementFileName = signedAgreement.FileName;
                }

                _context.Add(contract);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contract created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Clients = _context.Clients.ToList();
            return View(contract);
        }

        // GET: Contracts/Edit/5 - Edit contract
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            ViewBag.Clients = _context.Clients.ToList();
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
                try
                {
                    // Handle new file upload if provided
                    if (signedAgreement != null && signedAgreement.Length > 0)
                    {
                        // Validate it's a PDF
                        var fileExtension = Path.GetExtension(signedAgreement.FileName).ToLower();
                        if (fileExtension != ".pdf")
                        {
                            ModelState.AddModelError("signedAgreement", "ONLY PDF files are allowed!");
                            ViewBag.Clients = _context.Clients.ToList();
                            return View(contract);
                        }

                        // Delete old file if it exists
                        if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath,
                                contract.SignedAgreementPath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Save new file
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + signedAgreement.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await signedAgreement.CopyToAsync(fileStream);
                        }

                        contract.SignedAgreementPath = "/uploads/" + uniqueFileName;
                        contract.SignedAgreementFileName = signedAgreement.FileName;
                    }

                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Contract updated!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.ContractId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Clients = _context.Clients.ToList();
            return View(contract);
        }

        // GET: Contracts/Search - Search/filter contracts
        public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate, string? status)
        {
            // Start with all contracts
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            // Filter by start date if provided
            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

            // Filter by end date if provided
            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value);
            }

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }

            var results = await query.ToListAsync();

            // Pass data to the view for display
            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;
            ViewBag.CurrentStatus = status;
            ViewBag.Statuses = new List<string> { "Draft", "Active", "Expired", "OnHold" };

            return View(results);
        }

        // GET: Contracts/DownloadFile/5 - Download the signed agreement PDF
        public async Task<IActionResult> DownloadFile(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
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

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.ContractId == id);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Models;
using TechMoveGLMS.Services;

namespace TechMoveGLMS.Controllers
{
    // This controller handles service requests under contracts
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService; // used for currency conversion

        public ServiceRequestsController(ApplicationDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        // GET: ServiceRequests/Create - Show form to create service request
        [HttpGet]
        public async Task<IActionResult> Create(int contractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null)
            {
                return NotFound();
            }

            // WORKFLOW RULE: Cannot create service request if contract is Expired or On Hold
            if (contract.Status == "Expired" || contract.Status == "OnHold")
            {
                TempData["Error"] = $"Cannot create service request. This contract is {contract.Status}.";
                return RedirectToAction("Details", "Contracts", new { id = contractId });
            }

            // Get current exchange rate from the API
            var currentRate = await _currencyService.GetUsdToZarRateAsync();

            ViewBag.Contract = contract;
            ViewBag.CurrentRate = currentRate;
            ViewBag.ContractId = contractId;

            return View();  // Return empty view, model will be bound from form
        }
        // POST: ServiceRequests/Create - Save the service request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int contractId, [Bind("Description,AmountUSD")] ServiceRequest serviceRequest)
        {
            var contract = await _context.Contracts.FindAsync(contractId);

            if (contract == null)
            {
                return NotFound();
            }

            // Double-check the workflow rule
            if (contract.Status == "Expired" || contract.Status == "OnHold")
            {
                ModelState.AddModelError("", $"Cannot create request. Contract is {contract.Status}.");
                ViewBag.Contract = contract;
                ViewBag.CurrentRate = await _currencyService.GetUsdToZarRateAsync();
                return View(serviceRequest);
            }

            if (ModelState.IsValid)
            {
                // Get current exchange rate from API
                var exchangeRate = await _currencyService.GetUsdToZarRateAsync();

                // Convert USD to ZAR
                var amountZAR = _currencyService.ConvertUsdToZar(serviceRequest.AmountUSD, exchangeRate);

                // Fill in the rest of the service request
                serviceRequest.ContractId = contractId;
                serviceRequest.AmountZAR = amountZAR;
                serviceRequest.ExchangeRateUsed = exchangeRate;
                serviceRequest.Status = "Pending";
                serviceRequest.CreatedAt = DateTime.Now;

                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Service request created! ${serviceRequest.AmountUSD} USD = R{amountZAR} ZAR (Rate: {exchangeRate})";
                return RedirectToAction("Details", "Contracts", new { id = contractId });
            }

            ViewBag.Contract = contract;
            ViewBag.CurrentRate = await _currencyService.GetUsdToZarRateAsync();
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Edit/5 - Edit service request
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(s => s.ServiceRequestId == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // POST: ServiceRequests/Edit/5 - Save edited service request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceRequestId,ContractId,Description,AmountUSD,AmountZAR,Status")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.ServiceRequestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceRequest);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Service request updated!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(serviceRequest.ServiceRequestId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Contracts", new { id = serviceRequest.ContractId });
            }
            return View(serviceRequest);
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.ServiceRequestId == id);
        }
    }
}
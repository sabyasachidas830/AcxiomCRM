using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcxiomCRM.Data;
using AcxiomCRM.Models;

namespace AcxiomCRM.Controllers;

[Authorize]
public class OpportunitiesController : Controller
{
    private readonly ApplicationDbContext _context;

    public OpportunitiesController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        return View(await _context.Opportunities.Include(o => o.Customer).ToListAsync());
    }

    public IActionResult Create()
    {
        ViewBag.Customers = _context.Customers.ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Opportunity opportunity)
    {
        if (ModelState.IsValid)
        {
            opportunity.AssignedTo = User.Identity?.Name;
            _context.Add(opportunity);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = User.Identity?.Name,
                Action = "Create",
                EntityName = "Opportunity",
                RecordId = opportunity.OpportunityId.ToString(),
                NewValue = $"Amount: {opportunity.Amount}, Stage: {opportunity.Stage}"
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Customers = _context.Customers.ToList();
        return View(opportunity);
    }
}
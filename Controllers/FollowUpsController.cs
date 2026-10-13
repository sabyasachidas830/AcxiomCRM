using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcxiomCRM.Data;
using AcxiomCRM.Models;

namespace AcxiomCRM.Controllers;

[Authorize]
public class FollowUpsController : Controller
{
    private readonly ApplicationDbContext _context;

    public FollowUpsController(ApplicationDbContext context) => _context = context;

    // GET: /FollowUps
    public async Task<IActionResult> Index()
    {
        var followUps = await _context.FollowUps.ToListAsync();
        return View(followUps);
    }

    // GET: /FollowUps/Create
    public IActionResult Create()
    {
        ViewBag.Customers = _context.Customers.ToList();
        return View();
    }

    // POST: /FollowUps/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FollowUp followUp)
    {
        // Business Rule: Follow-up date cannot be earlier than today for a new/planned follow-up
        if (followUp.Status == "Planned" && followUp.FollowUpDate.Date < DateTime.UtcNow.Date)
        {
            ModelState.AddModelError("FollowUpDate", "Follow-up date cannot be earlier than today.");
        }

        if (ModelState.IsValid)
        {
            followUp.AssignedTo = User.Identity?.Name ?? "Sales Executive";
            _context.Add(followUp);
            await _context.SaveChangesAsync();

            // Audit Log
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = User.Identity?.Name,
                Action = "Create",
                EntityName = "FollowUp",
                RecordId = followUp.FollowUpId.ToString(),
                NewValue = $"Type: {followUp.FollowUpType}, Date: {followUp.FollowUpDate:yyyy-MM-dd}"
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Customers = _context.Customers.ToList();
        return View(followUp);
    }

    // POST: /FollowUps/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var item = await _context.FollowUps.FindAsync(id);
        if (item == null) return NotFound();

        item.Status = "Completed";
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = User.Identity?.Name,
            Action = "Update",
            EntityName = "FollowUp",
            RecordId = id.ToString(),
            NewValue = "Status marked as Completed"
        });
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
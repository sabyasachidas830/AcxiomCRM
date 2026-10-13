using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcxiomCRM.Data;
using AcxiomCRM.Models;

namespace AcxiomCRM.Controllers;

[Authorize]
public class LeadsController : Controller
{
    private readonly ApplicationDbContext _context;

    public LeadsController(ApplicationDbContext context) => _context = context;

    // GET: /Leads
    // GET: /Leads
public async Task<IActionResult> Index()
{
    var query = _context.Leads.AsQueryable();

    if (User.IsInRole("SalesExecutive"))
    {
        query = query.Where(l => l.AssignedTo == User.Identity!.Name);
    }

    return View(await query.ToListAsync());
}
    // GET: /Leads/Create
    public IActionResult Create() => View();

    // POST: /Leads/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Lead lead)
    {
        if (ModelState.IsValid)
        {
            lead.AssignedTo = User.Identity?.Name ?? "Sales Executive";
            _context.Add(lead);
            await _context.SaveChangesAsync();

            // Audit
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = User.Identity?.Name,
                Action = "Create",
                EntityName = "Lead",
                RecordId = lead.LeadId.ToString(),
                NewValue = $"Lead: {lead.LeadName}, Status: {lead.Status}"
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        return View(lead);
    }

    // POST: /Leads/Convert/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Convert(int id)
    {
        var lead = await _context.Leads.FindAsync(id);
        if (lead == null || lead.Status == "Converted") return NotFound();

        // Check if customer already exists with this email
        var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == lead.Email);
        if (existingCustomer == null)
        {
            var customer = new Customer
            {
                CustomerCode = "CUST-" + lead.LeadCode,
                CustomerName = lead.LeadName,
                Email = lead.Email,
                Phone = lead.Phone,
                CompanyName = lead.CompanyName,
                CreatedBy = User.Identity?.Name
            };
            _context.Customers.Add(customer);
        }

        lead.Status = "Converted";

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = User.Identity?.Name,
            Action = "Convert",
            EntityName = "Lead",
            RecordId = lead.LeadId.ToString(),
            NewValue = $"Converted lead {lead.LeadName} to Customer"
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
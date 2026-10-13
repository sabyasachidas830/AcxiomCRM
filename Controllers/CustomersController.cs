using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcxiomCRM.Data;
using AcxiomCRM.Models;

namespace AcxiomCRM.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Customers
    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Customers.AsQueryable();

        // Role-based data scoping: SalesExecutive sees only their own customers
        if (User.IsInRole("SalesExecutive"))
        {
            query = query.Where(c => c.CreatedBy == User.Identity!.Name);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.CustomerName.Contains(search) || 
                                     c.Email.Contains(search) || 
                                     c.Phone.Contains(search) || 
                                     (c.CompanyName != null && c.CompanyName.Contains(search)));
        }

        return View(await query.ToListAsync());
    }

    // GET: /Customers/Create
    public IActionResult Create() => View();

    // POST: /Customers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
        {
            ModelState.AddModelError("Email", "A customer with this email already exists.");
        }
        if (await _context.Customers.AnyAsync(c => c.Phone == customer.Phone))
        {
            ModelState.AddModelError("Phone", "A customer with this phone number already exists.");
        }

        if (ModelState.IsValid)
        {
            customer.CreatedBy = User.Identity?.Name ?? "System";
            _context.Add(customer);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = User.Identity?.Name,
                Action = "Create",
                EntityName = "Customer",
                RecordId = customer.CustomerId.ToString(),
                NewValue = $"Name: {customer.CustomerName}, Email: {customer.Email}",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(customer);
    }

    // GET: /Customers/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    // POST: /Customers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer customer)
    {
        if (id != customer.CustomerId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();

                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = User.Identity?.Name,
                    Action = "Update",
                    EntityName = "Customer",
                    RecordId = customer.CustomerId.ToString(),
                    NewValue = $"Updated: {customer.CustomerName}",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Customers.Any(e => e.CustomerId == customer.CustomerId)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(customer);
    }

    // POST: /Customers/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            customer.Status = "Deactivated";
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = User.Identity?.Name,
                Action = "Delete",
                EntityName = "Customer",
                RecordId = id.ToString(),
                NewValue = "Status set to Deactivated",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
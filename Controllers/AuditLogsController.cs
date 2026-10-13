using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcxiomCRM.Data;

namespace AcxiomCRM.Controllers;

[Authorize]
public class AuditLogsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuditLogsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var logs = await _context.AuditLogs
            .OrderByDescending(l => l.CreatedDate)
            .Take(50)
            .ToListAsync();
        return View(logs);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcxiomCRM.Data;

namespace AcxiomCRM.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;
    public ReportsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        // Pipeline Stage Breakdown
        ViewBag.PipelineData = await _context.Opportunities
            .GroupBy(o => o.Stage)
            .Select(g => new { Stage = g.Key, Count = g.Count(), TotalValue = g.Sum(x => x.Amount) })
            .ToListAsync();

        // Lead Conversion Metrics
        int totalLeads = await _context.Leads.CountAsync();
        int convertedLeads = await _context.Leads.CountAsync(l => l.Status == "Converted");
        ViewBag.TotalLeads = totalLeads;
        ViewBag.ConvertedLeads = convertedLeads;
        ViewBag.ConversionRate = totalLeads > 0 ? (convertedLeads * 100.0 / totalLeads) : 0;

        return View();
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcxiomCRM.Data;

namespace AcxiomCRM.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalCustomers = await _context.Customers.CountAsync();
        ViewBag.TotalLeads = await _context.Leads.CountAsync();
        ViewBag.OpenOpportunities = await _context.Opportunities.CountAsync(o => o.Status == "Open");
        ViewBag.TotalPipelineValue = await _context.Opportunities
            .Where(o => o.Status == "Open")
            .SumAsync(o => (decimal?)o.Amount) ?? 0;

        // Pipeline stage metrics for Chart.js
        ViewBag.StageQualification = await _context.Opportunities.CountAsync(o => o.Stage == "Qualification");
        ViewBag.StageProposal = await _context.Opportunities.CountAsync(o => o.Stage == "Proposal");
        ViewBag.StageNegotiation = await _context.Opportunities.CountAsync(o => o.Stage == "Negotiation");
        ViewBag.StageWon = await _context.Opportunities.CountAsync(o => o.Stage == "Won");
        ViewBag.StageLost = await _context.Opportunities.CountAsync(o => o.Stage == "Lost");

        return View();
    }
}
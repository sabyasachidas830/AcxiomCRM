using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AcxiomCRM.Data;
using AcxiomCRM.Models;

namespace AcxiomCRM.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class LeadsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public LeadsApiController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeadDto>>> GetAll()
    {
        var leads = await _context.Leads
            .Select(l => new LeadDto
            {
                LeadId = l.LeadId,
                LeadCode = l.LeadCode,
                LeadName = l.LeadName,
                Email = l.Email,
                Phone = l.Phone,
                Status = l.Status,
                ExpectedValue = l.ExpectedValue
            }).ToListAsync();
        return Ok(leads);
    }
}
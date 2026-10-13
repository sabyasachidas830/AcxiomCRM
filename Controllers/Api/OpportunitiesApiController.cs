using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AcxiomCRM.Data;
using AcxiomCRM.Models;

namespace AcxiomCRM.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class OpportunitiesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public OpportunitiesApiController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OpportunityDto>>> GetAll()
    {
        var opps = await _context.Opportunities
            .Select(o => new OpportunityDto
            {
                OpportunityId = o.OpportunityId,
                OpportunityName = o.OpportunityName,
                Amount = o.Amount,
                Stage = o.Stage,
                Probability = o.Probability,
                ExpectedCloseDate = o.ExpectedCloseDate
            }).ToListAsync();
        return Ok(opps);
    }
}
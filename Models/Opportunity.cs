using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models;

public class Opportunity : IValidatableObject
{
    public int OpportunityId { get; set; }

    [Required(ErrorMessage = "Opportunity Name is required.")]
    [StringLength(150)]
    public string OpportunityName { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [Range(0.01, 100000000, ErrorMessage = "Opportunity Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Required]
    public string Stage { get; set; } = "Qualification"; // Qualification, Proposal, Negotiation, Won, Lost

    [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
    public int Probability { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime ExpectedCloseDate { get; set; }

    public string Status { get; set; } = "Open";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? AssignedTo { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status == "Open" && ExpectedCloseDate.Date < DateTime.UtcNow.Date)
        {
            yield return new ValidationResult(
                "Expected Close Date cannot be in the past.",
                new[] { nameof(ExpectedCloseDate) });
        }
    }
}
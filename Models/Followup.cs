using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models;

public class FollowUp : IValidatableObject
{
    public int FollowUpId { get; set; }
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime FollowUpDate { get; set; }

    [Required]
    public string FollowUpType { get; set; } = "Call"; // Call, Meeting, Email, Task

    public string? Remarks { get; set; }

    [Required]
    public string Status { get; set; } = "Planned"; // Planned, Completed, Missed, Cancelled

    public string? AssignedTo { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status == "Planned" && FollowUpDate.Date < DateTime.UtcNow.Date)
        {
            yield return new ValidationResult(
                "Follow-up date cannot be earlier than today.",
                new[] { nameof(FollowUpDate) });
        }
    }
}
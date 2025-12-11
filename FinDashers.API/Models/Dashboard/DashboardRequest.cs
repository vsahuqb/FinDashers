using System.ComponentModel.DataAnnotations;

namespace FinDashers.API.Models.Dashboard;

public class DashboardRequest
{
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public string? LocationId { get; set; }
}
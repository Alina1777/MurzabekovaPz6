
namespace ClinicCRM.Models;

public class AppointmentViewDto
{
    public string Patient { get; set; } = "";
    public string Doctor { get; set; } = "";
    public string Department { get; set; } = "";
    public decimal Cost { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = "";
}

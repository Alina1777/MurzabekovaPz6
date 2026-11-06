
namespace ClinicCRM.Models;

public class Appointment
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string PatientName { get; set; } = "";
    public decimal Cost { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = "Planned";
}

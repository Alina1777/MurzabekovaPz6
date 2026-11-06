
namespace ClinicCRM.Models;

public class AppointmentEventArgs : EventArgs
{
    public string DoctorName { get; }
    public string PatientName { get; }
    public decimal Cost { get; }
    public string Message { get; }

    public AppointmentEventArgs(
        string doctorName,
        string patientName,
        decimal cost,
        string message)
    {
        DoctorName = doctorName;
        PatientName = patientName;
        Cost = cost;
        Message = message;
    }
}

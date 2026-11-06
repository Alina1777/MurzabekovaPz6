
using ClinicCRM.Data;
using ClinicCRM.Models;

namespace ClinicCRM.Services;

public class ClinicManager
{
    private readonly ClinicDataService _dataService;

    public event EventHandler<AppointmentEventArgs>?
        AppointmentRegistered;

    public Action<string>? Logger { get; set; }

    public ClinicManager(ClinicDataService dataService)
    {
        _dataService = dataService;
    }

    public void RegisterAppointment(
        Appointment appointment,
        Doctor doctor)
    {
        _dataService.AddAppointment(appointment);

        string message =
            $"Приём зарегистрирован: {appointment.PatientName}, " +
            $"врач: {doctor.FullName}, стоимость: {appointment.Cost:N2}";

        AppointmentRegistered?.Invoke(
            this,
            new AppointmentEventArgs(
                doctor.FullName,
                appointment.PatientName,
                appointment.Cost,
                message));

        Logger?.Invoke(message);
    }

    public List<Appointment> GetCompletedAppointments()
    {
        Predicate<Appointment> predicate =
            a => a.Status == "Completed";

        return _dataService.GetAppointments()
            .Where(a => predicate(a))
            .ToList();
    }

    public decimal GetDiscountedCost(Appointment appointment)
    {
        Func<Appointment, decimal> discount =
            a => a.Cost * 0.9m;

        return discount(appointment);
    }

    public void LogAction(string message)
    {
        Logger?.Invoke(message);
    }

    public Func<Appointment, bool> CreateBudgetFilter(
        decimal minCost)
    {
        return appointment => appointment.Cost >= minCost;
    }
}

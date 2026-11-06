
using ClinicCRM.Interfaces;

namespace ClinicCRM.Models;

public class Doctor : MedicalEmployee, IExportable, ICloneable
{
    public string Category { get; private set; }
    public int DepartmentId { get; private set; }

    public MedicalLicense License { get; set; }

    public Doctor(
        int id,
        string fullName,
        decimal baseSalary,
        string category,
        int departmentId,
        MedicalLicense? license = null)
        : base(id, fullName, baseSalary)
    {
        Category = category;
        DepartmentId = departmentId;

        License = license ??
            new MedicalLicense(
                "LIC-" + id,
                DateTime.Today.AddYears(3));
    }

    public override decimal CalculateMonthlyPayout(int appointmentsCount)
    {
        return BaseSalary + appointmentsCount * 450m;
    }

    public override string GetInfo()
    {
        return base.GetInfo() + $" | Категория: {Category}";
    }

    string IExportable.GetSummary()
    {
        return
            $"ОТЧЁТ ВРАЧА\n\n" +
            $"ФИО: {FullName}\n" +
            $"Категория: {Category}\n" +
            $"Оклад: {BaseSalary:N2}\n" +
            $"Лицензия: {License}";
    }

    public virtual object Clone()
    {
        var clone = (Doctor)MemberwiseClone();
        clone.License = License.Clone();
        return clone;
    }
}

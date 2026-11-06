
namespace ClinicCRM.Models;

public abstract class MedicalEmployee : IComparable
{
    public int Id { get; private set; }
    public string FullName { get; private set; }
    public decimal BaseSalary { get; private set; }

    protected MedicalEmployee(int id, string fullName, decimal baseSalary)
    {
        Id = id;
        FullName = fullName;
        BaseSalary = baseSalary;
    }

    public virtual string GetInfo()
    {
        return $"{FullName}, оклад: {BaseSalary:N2}";
    }

    public abstract decimal CalculateMonthlyPayout(int appointmentsCount);

    public override string ToString()
    {
        return $"#{Id} | {FullName} | {BaseSalary:N2}";
    }

    public override bool Equals(object? obj)
    {
        return obj is MedicalEmployee other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public int CompareTo(object? obj)
    {
        if (obj is not MedicalEmployee other)
            throw new ArgumentException("Некорректный объект.");

        return other.BaseSalary.CompareTo(BaseSalary);
    }
}

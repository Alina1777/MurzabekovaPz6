
namespace ClinicCRM.Models;

public class Surgeon : Doctor
{
    private int _complexOperationsExperience;

    public Surgeon(
        int id,
        string fullName,
        decimal baseSalary,
        string category,
        int departmentId,
        int experience,
        MedicalLicense? license = null)
        : base(id, fullName, baseSalary, category, departmentId, license)
    {
        _complexOperationsExperience = experience;
    }

    public override decimal CalculateMonthlyPayout(int appointmentsCount)
    {
        decimal categoryBonus = Category switch
        {
            "Высшая" => 10000m,
            "Первая" => 5000m,
            _ => 0m
        };

        decimal experienceBonus =
            _complexOperationsExperience * 500m;

        return BaseSalary +
               appointmentsCount * 1200m +
               categoryBonus +
               experienceBonus;
    }

    public override string GetInfo()
    {
        return base.GetInfo() +
               $" | Хирург | Стаж операций: {_complexOperationsExperience}";
    }

    public override object Clone()
    {
        return base.Clone();
    }
}

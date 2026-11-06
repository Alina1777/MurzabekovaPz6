
namespace ClinicCRM.Models;

public class MedicalLicense
{
    public string Number { get; set; }
    public DateTime ExpiryDate { get; set; }

    public MedicalLicense(string number, DateTime expiryDate)
    {
        Number = number;
        ExpiryDate = expiryDate;
    }

    public MedicalLicense Clone()
    {
        return new MedicalLicense(Number, ExpiryDate);
    }

    public override string ToString()
    {
        return $"{Number}, до {ExpiryDate:dd.MM.yyyy}";
    }
}

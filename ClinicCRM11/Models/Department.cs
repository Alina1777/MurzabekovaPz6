
namespace ClinicCRM.Models;

public class Department
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal BaseRate { get; set; }

    public override string ToString() => Title;
}


using ClinicCRM.Data;
using ClinicCRM.Forms;

namespace ClinicCRM;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            DatabaseInitializer.Initialize();
            Application.Run(new FormMain());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "Ошибка CLINIC CRM",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}

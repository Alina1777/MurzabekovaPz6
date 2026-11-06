
using ClinicCRM.Data;
using ClinicCRM.Interfaces;
using ClinicCRM.Models;
using ClinicCRM.Services;

namespace ClinicCRM.Forms;

public class FormMain : Form
{
    private readonly ClinicDataService _service;
    private readonly ClinicManager _manager;

    private List<MedicalEmployee> _employees = new();

    private readonly TabControl tabs = new();

    private readonly DataGridView dgvDoctors = new();
    private readonly DataGridView dgvReport = new();
    private readonly DataGridView dgvAnalytics = new();

    private readonly ComboBox cmbCategory = new();
    private readonly ComboBox cmbDepartment = new();
    private readonly ComboBox cmbDoctor = new();

    private readonly TextBox txtName = new();
    private readonly TextBox txtPatientSearch = new();

    private readonly NumericUpDown numSalary = new();
    private readonly NumericUpDown numCost = new();

    private readonly DateTimePicker dtpDate = new();

    private readonly CheckBox chkCompleted = new();

    private readonly Label lblRevenue = new();

    private readonly ListBox lstEvents = new();

    private List<Doctor> _doctors = new();
    private List<Department> _departments = new();

    public FormMain()
    {
        Text = "CLINIC CRM — Система управления клиникой";
        Width = 1200;
        Height = 750;
        StartPosition = FormStartPosition.CenterScreen;

        _service = new ClinicDataService(
            DatabaseInitializer.ConnectionString);

        _manager = new ClinicManager(_service);

        _manager.AppointmentRegistered += OnAppointmentRegistered;
        _manager.Logger = Log;

        InitializeInterface();
        LoadData();
    }

    private void InitializeInterface()
    {
        tabs.Dock = DockStyle.Fill;

        tabs.TabPages.Add(CreateDoctorsTab());
        tabs.TabPages.Add(CreateAppointmentTab());
        tabs.TabPages.Add(CreateReportTab());
        tabs.TabPages.Add(CreateAnalyticsTab());

        Controls.Add(tabs);
    }

    private TabPage CreateDoctorsTab()
    {
        var page = new TabPage("Врачи и ООП-модель");

        ConfigureGrid(dgvDoctors);

        txtName.PlaceholderText = "ФИО врача";
        txtName.Width = 220;

        cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCategory.Items.AddRange(new object[]
        {
            "Высшая", "Первая"
        });
        cmbCategory.SelectedIndex = 0;
        cmbCategory.Width = 120;

        numSalary.Minimum = 1000;
        numSalary.Maximum = 1000000;
        numSalary.Value = 60000;
        numSalary.Width = 130;

        cmbDepartment.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbDepartment.Width = 160;

        var addButton = Button("Добавить врача", (_, _) => AddDoctor());

        var sortButton = Button(
            "Сортировать по окладу (IComparable)",
            (_, _) => SortDoctors());

        var cloneButton = Button(
            "Клонировать (ICloneable)",
            (_, _) => CloneDoctor());

        var exportButton = Button(
            "Экспорт отчёта",
            (_, _) => ExportDoctor());

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 100,
            Padding = new Padding(8),
            AutoScroll = true
        };

        panel.Controls.AddRange(new Control[]
        {
            txtName, cmbCategory, numSalary,
            cmbDepartment, addButton, sortButton,
            cloneButton, exportButton
        });

        dgvDoctors.Dock = DockStyle.Fill;

        page.Controls.Add(dgvDoctors);
        page.Controls.Add(panel);

        return page;
    }

    private TabPage CreateAppointmentTab()
    {
        var page = new TabPage("Запись на приём и Events");

        cmbDoctor.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbDoctor.Width = 250;

        txtPatientSearch.PlaceholderText = "ФИО пациента";
        txtPatientSearch.Width = 200;

        numCost.Minimum = 100;
        numCost.Maximum = 1000000;
        numCost.Value = 2500;
        numCost.Width = 130;

        dtpDate.Format = DateTimePickerFormat.Custom;
        dtpDate.CustomFormat = "dd.MM.yyyy HH:mm";
        dtpDate.Width = 170;

        var registerButton = Button(
            "Зарегистрировать приём",
            (_, _) => RegisterAppointment());

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 90,
            Padding = new Padding(8),
            AutoScroll = true
        };

        panel.Controls.AddRange(new Control[]
        {
            cmbDoctor,
            txtPatientSearch,
            numCost,
            dtpDate,
            registerButton
        });

        lstEvents.Dock = DockStyle.Fill;

        page.Controls.Add(lstEvents);
        page.Controls.Add(panel);

        return page;
    }

    private TabPage CreateReportTab()
    {
        var page = new TabPage("LINQ-аналитика и DTO");

        ConfigureGrid(dgvReport);

        cmbDepartment.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbDepartment.Width = 180;

        var search = new TextBox
        {
            PlaceholderText = "Поиск пациента",
            Width = 180
        };

        chkCompleted.Text = "Только завершенные";
        chkCompleted.AutoSize = true;

        lblRevenue.AutoSize = true;
        lblRevenue.Font = new Font("Arial", 11, FontStyle.Bold);

        var filterButton = Button(
            "Применить фильтры",
            (_, _) => FilterReport(search.Text));

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 75,
            Padding = new Padding(8)
        };

        panel.Controls.AddRange(new Control[]
        {
            cmbDepartment,
            search,
            chkCompleted,
            filterButton,
            lblRevenue
        });

        dgvReport.Dock = DockStyle.Fill;

        page.Controls.Add(dgvReport);
        page.Controls.Add(panel);

        return page;
    }

    private TabPage CreateAnalyticsTab()
    {
        var page = new TabPage("Группировка и множества");

        ConfigureGrid(dgvAnalytics);

        var revenueButton = Button(
            "Выручка по врачам (GroupBy)",
            (_, _) => ShowRevenue());

        var intersectButton = Button(
            "Пересечение пациентов (Intersect)",
            (_, _) => ShowPatients(_service.GetIntersectPatients(),
                "Посещали терапию и хирургию"));

        var exceptButton = Button(
            "Исключение (Except)",
            (_, _) => ShowPatients(_service.GetExceptPatients(),
                "Посещали только терапию"));

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 65,
            Padding = new Padding(8)
        };

        panel.Controls.AddRange(new Control[]
        {
            revenueButton, intersectButton, exceptButton
        });

        dgvAnalytics.Dock = DockStyle.Fill;

        page.Controls.Add(dgvAnalytics);
        page.Controls.Add(panel);

        return page;
    }

    private void LoadData()
    {
        _departments = _service.GetDepartments();
        _doctors = _service.GetDoctors();

        _employees = _doctors
            .Cast<MedicalEmployee>()
            .ToList();

        cmbDepartment.DataSource = null;
        cmbDepartment.DataSource = _departments;
        cmbDepartment.DisplayMember = "Title";

        cmbDepartment.SelectedIndex = -1;

        var doctorDepartments = _departments.ToList();



        cmbDoctor.DataSource = null;
        cmbDoctor.DataSource = _doctors;
        cmbDoctor.DisplayMember = "FullName";

        RefreshDoctorGrid();
        RefreshReport();
    }

    private void AddDoctor()
    {
        string name = txtName.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Введите ФИО врача.");
            return;
        }

        if (cmbDepartment.SelectedItem is not Department department)
        {
            MessageBox.Show("Выберите отделение.");
            return;
        }

        var doctor = new Doctor(
            0,
            name,
            numSalary.Value,
            cmbCategory.Text,
            department.Id);

        _service.AddDoctor(doctor);

        Log("Добавлен врач: " + name);

        LoadData();

        MessageBox.Show("Врач успешно добавлен!");
    }

    private void SortDoctors()
    {
        _employees.Sort();

        dgvDoctors.DataSource = null;
        dgvDoctors.DataSource = _employees.Select(e => new
        {
            e.Id,
            e.FullName,
            e.BaseSalary
        }).ToList();

        Log("Врачи отсортированы по убыванию оклада.");
    }

    private void RefreshDoctorGrid()
    {
        dgvDoctors.DataSource = null;

        dgvDoctors.DataSource = _employees.Select(e => new
        {
            e.Id,
            e.FullName,
            e.BaseSalary
        }).ToList();
    }

    private void CloneDoctor()
    {
        if (dgvDoctors.CurrentRow == null)
            return;

        int id = Convert.ToInt32(
            dgvDoctors.CurrentRow.Cells["Id"].Value);

        var doctor = _doctors.FirstOrDefault(d => d.Id == id);

        if (doctor == null)
            return;

        var clone = (Doctor)doctor.Clone();

        string originalLicense = doctor.License.Number;

        clone.License.Number = "CLONE-" + originalLicense;

        bool independent =
            doctor.License.Number != clone.License.Number;

        MessageBox.Show(
            $"Оригинальная лицензия: {doctor.License.Number}\n" +
            $"Лицензия клона: {clone.License.Number}\n" +
            $"Глубокое копирование: {(independent ? "Успешно" : "Ошибка")}");

        Log("Выполнено глубокое клонирование врача.");
    }

    private void ExportDoctor()
    {
        if (dgvDoctors.CurrentRow == null)
            return;

        int id = Convert.ToInt32(
            dgvDoctors.CurrentRow.Cells["Id"].Value);

        var doctor = _doctors.FirstOrDefault(d => d.Id == id);

        if (doctor == null)
            return;

        string report = ((IExportable)doctor).GetSummary();

        using var dialog = new SaveFileDialog
        {
            Filter = "Текстовые файлы (*.txt)|*.txt",
            FileName = "DoctorReport.txt"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            File.WriteAllText(dialog.FileName, report);
            Log("Отчёт экспортирован.");
        }
    }

    private void RegisterAppointment()
    {
        if (cmbDoctor.SelectedItem is not Doctor doctor)
        {
            MessageBox.Show("Выберите врача.");
            return;
        }

        string patient = txtPatientSearch.Text.Trim();

        if (string.IsNullOrWhiteSpace(patient))
        {
            MessageBox.Show("Введите имя пациента.");
            return;
        }

        var appointment = new Appointment
        {
            DoctorId = doctor.Id,
            PatientName = patient,
            Cost = numCost.Value,
            Date = dtpDate.Value,
            Status = "Planned"
        };

        try
        {
            _manager.RegisterAppointment(appointment, doctor);

            RefreshReport();

            MessageBox.Show("Приём зарегистрирован!");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка");
        }
    }

    private void OnAppointmentRegistered(
        object? sender,
        AppointmentEventArgs e)
    {
        lstEvents.Items.Add(e.Message);
    }

    private void FilterReport(string patient)
    {
        var result = _service.GetAppointmentReport()
            .AsEnumerable();

        if (cmbDepartment.SelectedItem is Department department)
        {
            result = result.Where(a =>
                a.Department == department.Title);
        }

        if (!string.IsNullOrWhiteSpace(patient))
        {
            result = result.Where(a =>
                a.Patient.Contains(
                    patient,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (chkCompleted.Checked)
        {
            result = result.Where(a =>
                a.Status == "Completed");
        }

        var data = result.ToList();

        dgvReport.DataSource = data;

        lblRevenue.Text =
            $"Итоговая выручка: {data.Sum(a => a.Cost):N2}";

        Log("Фильтрация LINQ выполнена.");
    }

    private void RefreshReport()
    {
        var data = _service.GetAppointmentReport();

        dgvReport.DataSource = null;
        dgvReport.DataSource = data;

        lblRevenue.Text =
            $"Итоговая выручка: {data.Sum(a => a.Cost):N2}";
    }

    private void ShowRevenue()
    {
        dgvAnalytics.DataSource =
            _service.GetRevenueByDoctors();

        Log("Выполнена группировка GroupBy.");
    }

    private void ShowPatients(List<string> patients, string title)
    {
        dgvAnalytics.DataSource = patients
            .Select(p => new
            {
                Patient = p,
                Operation = title
            })
            .ToList();

        Log(title);
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => Log(message));
            return;
        }

        lstEvents.Items.Add(
            $"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    private static Button Button(
        string text,
        EventHandler handler)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Height = 35,
            Margin = new Padding(5)
        };

        button.Click += handler;

        return button;
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        grid.MultiSelect = false;
    }
}

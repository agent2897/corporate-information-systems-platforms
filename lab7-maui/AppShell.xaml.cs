using ProjectContractsMaui.Views;

namespace ProjectContractsMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        // Маршрути сторінок, що відкриваються поверх вкладок
        Routing.RegisterRoute("employee", typeof(EmployeeEditPage));
        Routing.RegisterRoute("project", typeof(ProjectDetailsPage));
        Routing.RegisterRoute("project-edit", typeof(ProjectEditPage));
    }
}

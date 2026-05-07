namespace HydraulicJournalApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(DeveloperDetailsPage), typeof(DeveloperDetailsPage));
        Routing.RegisterRoute(nameof(CustomerDetailsPage), typeof(CustomerDetailsPage));
        Routing.RegisterRoute(nameof(ProductDetailsPage), typeof(ProductDetailsPage));

        Routing.RegisterRoute(nameof(DeveloperEditPage), typeof(DeveloperEditPage));
        Routing.RegisterRoute(nameof(CustomerEditPage), typeof(CustomerEditPage));
        Routing.RegisterRoute(nameof(ProductEditPage), typeof(ProductEditPage));
    }
}
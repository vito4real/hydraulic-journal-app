namespace HydraulicJournalApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(DeveloperDetailsPage), typeof(DeveloperDetailsPage));
        Routing.RegisterRoute(nameof(CustomerDetailsPage), typeof(CustomerDetailsPage));
        Routing.RegisterRoute(nameof(ProductDetailsPage), typeof(ProductDetailsPage));
    }
}
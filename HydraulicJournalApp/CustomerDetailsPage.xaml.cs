using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

[QueryProperty(nameof(CustomerId), "customerId")]
public partial class CustomerDetailsPage : ContentPage
{
    private readonly DatabaseService _db;
    private int _customerId;

    public string CustomerId
    {
        set
        {
            if (int.TryParse(value, out var id))
            {
                _customerId = id;
            }
        }
    }

    public CustomerDetailsPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (_customerId <= 0)
            return;

        var customer = await _db.GetCustomerByIdAsync(_customerId);
        CustomerNameLabel.Text = customer?.Name ?? "Клиент";

        CustomerProductsList.ItemsSource = await _db.GetProductsByCustomerAsync(_customerId);
    }
}
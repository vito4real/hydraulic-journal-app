using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class CustomersPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AccessGuardService _accessGuard;

    public CustomersPage(DatabaseService db, AccessGuardService accessGuard)
    {
        InitializeComponent();
        _db = db;
        _accessGuard = accessGuard;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        CustomersList.ItemsSource = await _db.GetCustomersAsync();
    }

    private async void OnAddCustomer(object sender, EventArgs e)
    {
        try
        {
            if (!await _accessGuard.EnsureWriteAccessAsync(this))
                return;

            await _db.AddCustomerAsync(CustomerEntry.Text ?? string.Empty);
            CustomerEntry.Text = string.Empty;
            await LoadDataAsync();
            await DisplayAlert("Готово", "Клиент добавлен.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnCustomerSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection.FirstOrDefault() is not HydraulicJournalApp.Models.Customer customer)
                return;

            CustomersList.SelectedItem = null;

            await Shell.Current.GoToAsync($"{nameof(CustomerDetailsPage)}?customerId={customer.Id}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}
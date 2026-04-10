using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class CustomersPage : ContentPage
{
    private readonly DatabaseService _db;

    public CustomersPage(DatabaseService db)
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
        CustomersList.ItemsSource = await _db.GetCustomersAsync();
    }

    private async void OnAddCustomer(object sender, EventArgs e)
    {
        try
        {
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

    private async void OnOpenCustomerClicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.CommandParameter is not int customerId)
            return;

        await Shell.Current.GoToAsync($"{nameof(CustomerDetailsPage)}?customerId={customerId}");
    }
}
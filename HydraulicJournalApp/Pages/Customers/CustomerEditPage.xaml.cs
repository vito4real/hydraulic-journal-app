using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

[QueryProperty(nameof(CustomerId), "customerId")]
public partial class CustomerEditPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AccessGuardService _accessGuard;

    public int CustomerId { get; set; }

    public CustomerEditPage(DatabaseService db, AccessGuardService accessGuard)
    {
        InitializeComponent();
        _db = db;
        _accessGuard = accessGuard;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var customer = await _db.GetCustomerByIdAsync(CustomerId);

        if (customer == null)
        {
            await DisplayAlert("Ошибка", "Клиент не найден.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        CustomerNameEntry.Text = customer.Name;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (!await _accessGuard.EnsureWriteAccessAsync(this))
                return;

            await _db.UpdateCustomerAsync(CustomerId, CustomerNameEntry.Text ?? string.Empty);

            await DisplayAlert("Готово", "Клиент обновлён.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
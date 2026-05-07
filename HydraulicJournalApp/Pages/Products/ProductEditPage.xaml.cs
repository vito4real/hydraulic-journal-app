using HydraulicJournalApp.Models;
using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

[QueryProperty(nameof(ProductId), "productId")]
public partial class ProductEditPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AccessGuardService _accessGuard;

    private List<Customer> _customers = new();

    public int ProductId { get; set; }

    public ProductEditPage(DatabaseService db, AccessGuardService accessGuard)
    {
        InitializeComponent();
        _db = db;
        _accessGuard = accessGuard;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _customers = await _db.GetCustomersAsync();

        CustomerPicker.ItemsSource = _customers;
        CustomerPicker.ItemDisplayBinding = new Binding("Name");

        var product = await _db.GetProductByIdAsync(ProductId);

        if (product == null)
        {
            await DisplayAlert("Ошибка", "Изделие не найдено.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        DesignationEntry.Text = product.Designation;
        ProductNameEntry.Text = product.Name;
        CustomerPicker.SelectedItem = _customers.FirstOrDefault(x => x.Id == product.CustomerId);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (!await _accessGuard.EnsureWriteAccessAsync(this))
                return;

            if (CustomerPicker.SelectedItem is not Customer customer)
            {
                await DisplayAlert("Ошибка", "Выберите клиента.", "OK");
                return;
            }

            await _db.UpdateProductAsync(
                ProductId,
                DesignationEntry.Text ?? string.Empty,
                ProductNameEntry.Text ?? string.Empty,
                customer.Id);

            await DisplayAlert("Готово", "Изделие обновлено.", "OK");

            var newDesignation = Uri.EscapeDataString((DesignationEntry.Text ?? string.Empty).Trim());

            await Shell.Current.GoToAsync($"../../{nameof(ProductDetailsPage)}?designation={newDesignation}");
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
using HydraulicJournalApp.Models;
using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class DirectoriesPage : ContentPage
{
    private readonly DatabaseService _db;

    private List<Product> _products = new();
    private List<Customer> _customers = new();
    private List<Developer> _developers = new();

    public DirectoriesPage(DatabaseService db)
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
        _customers = await _db.GetCustomersAsync();
        _developers = await _db.GetDevelopersAsync();
        _products = await _db.GetProductsAsync();

        CustomerPicker.ItemsSource = _customers;
        CustomerPicker.ItemDisplayBinding = new Binding("Name");

        DeveloperPicker.ItemsSource = _developers;
        DeveloperPicker.ItemDisplayBinding = new Binding("FullName");

        ProductPicker.ItemsSource = _products;
        ProductPicker.ItemDisplayBinding = new Binding("Designation");

        if (KitTypePicker.SelectedIndex < 0)
            KitTypePicker.SelectedIndex = 0;
    }

    private async void OnAddCustomer(object sender, EventArgs e)
    {
        try
        {
            await _db.AddCustomerAsync(CustomerEntry.Text ?? string.Empty);
            CustomerEntry.Text = string.Empty;
            await LoadDataAsync();
            await DisplayAlert("Готово", "Заказчик добавлен.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnAddDeveloper(object sender, EventArgs e)
    {
        try
        {
            await _db.AddDeveloperAsync(DeveloperEntry.Text ?? string.Empty);
            DeveloperEntry.Text = string.Empty;
            await LoadDataAsync();
            await DisplayAlert("Готово", "Разработчик добавлен.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnAddProduct(object sender, EventArgs e)
    {
        try
        {
            if (CustomerPicker.SelectedItem is not Customer customer)
            {
                await DisplayAlert("Ошибка", "Выбери заказчика.", "OK");
                return;
            }

            var check = await _db.CheckDesignationAsync(DesignationEntry.Text ?? string.Empty, customer.Id);

            if (!check.IsAllowed)
            {
                await DisplayAlert("Ошибка", check.Message, "OK");
                return;
            }

            await _db.AddProductAsync(
                DesignationEntry.Text ?? string.Empty,
                ProductNameEntry.Text ?? string.Empty,
                customer.Id);

            DesignationEntry.Text = string.Empty;
            ProductNameEntry.Text = string.Empty;

            await LoadDataAsync();
            await DisplayAlert("Готово", "Изделие добавлено.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnAddJournalEntry(object sender, EventArgs e)
    {
        try
        {
            if (ProductPicker.SelectedItem is not Product product)
            {
                await DisplayAlert("Ошибка", "Выбери изделие.", "OK");
                return;
            }

            if (DeveloperPicker.SelectedItem is not Developer developer)
            {
                await DisplayAlert("Ошибка", "Выбери разработчика.", "OK");
                return;
            }

            if (KitTypePicker.SelectedIndex < 0)
            {
                await DisplayAlert("Ошибка", "Выбери тип комплекта.", "OK");
                return;
            }

            var kitType = KitTypePicker.SelectedIndex == 0
                ? KitType.Experimental
                : KitType.Control;

            await _db.AddJournalEntryAsync(
                product.Id,
                developer.Id,
                IssueDatePicker.Date ?? DateTime.Now,
                kitType);

            await DisplayAlert("Готово", "Запись журнала добавлена.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}
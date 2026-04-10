using HydraulicJournalApp.Models;
using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class JournalEntryPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AccessGuardService _accessGuard;
    private List<Customer> _customers = new();
    private List<Developer> _developers = new();

    public JournalEntryPage(DatabaseService db, AccessGuardService accessGuard)
    {
        InitializeComponent();
        _db = db;
        _accessGuard = accessGuard;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
        SetProductFieldsEditable(true);
    }

    private async Task LoadDataAsync()
    {
        _customers = await _db.GetCustomersAsync();
        _developers = await _db.GetDevelopersAsync();

        CustomerPicker.ItemsSource = _customers;
        CustomerPicker.ItemDisplayBinding = new Binding("Name");

        DeveloperPicker.ItemsSource = _developers;
        DeveloperPicker.ItemDisplayBinding = new Binding("FullName");

        if (KitTypePicker.SelectedIndex < 0)
            KitTypePicker.SelectedIndex = 0;
    }

    private void SetProductFieldsEditable(bool isEditable)
    {
        ProductNameEntry.IsEnabled = isEditable;
        CustomerPicker.IsEnabled = isEditable;
    }

    private async void OnDesignationUnfocused(object? sender, FocusEventArgs e)
    {
        try
        {
            var designation = (DesignationEntry.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(designation))
            {
                ProductNameEntry.Text = string.Empty;
                CustomerPicker.SelectedItem = null;
                SetProductFieldsEditable(true);
                return;
            }

            var existingProduct = await _db.GetProductByDesignationAsync(designation);

            if (existingProduct == null)
            {
                ProductNameEntry.Text = string.Empty;
                CustomerPicker.SelectedItem = null;
                SetProductFieldsEditable(true);
                return;
            }

            ProductNameEntry.Text = existingProduct.Name;

            var customer = _customers.FirstOrDefault(x => x.Id == existingProduct.CustomerId);
            CustomerPicker.SelectedItem = customer;

            SetProductFieldsEditable(false);
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
            if (!await _accessGuard.EnsureWriteAccessAsync(this))
                return;

            if (CustomerPicker.SelectedItem is not Customer customer)
            {
                await DisplayAlert("Ошибка", "Выбери клиента.", "OK");
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

            await _db.AddJournalEntryWithProductAsync(
                DesignationEntry.Text ?? string.Empty,
                ProductNameEntry.Text ?? string.Empty,
                customer.Id,
                developer.Id,
                IssueDatePicker.Date ?? DateTime.Now,
                kitType);

            ResetForm();

            await DisplayAlert("Готово", "Запись журнала сохранена.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private void ResetForm()
    {
        DesignationEntry.Text = string.Empty;
        ProductNameEntry.Text = string.Empty;

        CustomerPicker.SelectedItem = null;
        DeveloperPicker.SelectedItem = null;

        IssueDatePicker.Date = DateTime.Today;
        KitTypePicker.SelectedIndex = 0;

        SetProductFieldsEditable(true);
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
        ResetForm();
    }
}
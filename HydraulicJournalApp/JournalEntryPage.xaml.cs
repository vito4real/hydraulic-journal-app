using HydraulicJournalApp.Models;
using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class JournalEntryPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AccessGuardService _accessGuard;

    private List<Customer> _customers = new();
    private List<Developer> _developers = new();
    private List<Product> _products = new();

    private Customer? _selectedCustomer;
    private Developer? _selectedDeveloper;

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
        _products = await _db.GetProductsAsync();

        CustomerResultsList.ItemsSource = _customers;
        DeveloperResultsList.ItemsSource = _developers;
        DesignationResultsList.ItemsSource = _products;
        ProductNameResultsList.ItemsSource = _products;

        if (KitTypePicker.SelectedIndex < 0)
            KitTypePicker.SelectedIndex = 0;
    }

    private void SetProductFieldsEditable(bool isEditable)
    {
        ProductNameEntry.IsEnabled = isEditable;
        CustomerSearchEntry.IsEnabled = isEditable;
    }

    private async void OnDesignationUnfocused(object? sender, FocusEventArgs e)
    {
        try
        {
            DesignationResultsBorder.IsVisible = false;

            var designation = (DesignationEntry.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(designation))
            {
                ProductNameEntry.Text = string.Empty;
                _selectedCustomer = null;
                CustomerSearchEntry.Text = string.Empty;
                CustomerResultsBorder.IsVisible = false;
                SetProductFieldsEditable(true);
                return;
            }

            var existingProduct = await _db.GetProductByDesignationAsync(designation);

            if (existingProduct == null)
            {
                _selectedCustomer = null;
                CustomerSearchEntry.Text = string.Empty;
                CustomerResultsBorder.IsVisible = false;
                SetProductFieldsEditable(true);
                return;
            }

            ProductNameEntry.Text = existingProduct.Name;

            var customer = _customers.FirstOrDefault(x => x.Id == existingProduct.CustomerId);
            _selectedCustomer = customer;
            CustomerSearchEntry.Text = customer?.Name ?? string.Empty;
            CustomerResultsBorder.IsVisible = false;

            SetProductFieldsEditable(true);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private void OnDesignationFocused(object sender, FocusEventArgs e)
    {
        ApplyDesignationFilter();
    }

    private void OnProductNameFocused(object sender, FocusEventArgs e)
    {
        ApplyProductNameFilter();
    }

    private void OnDesignationTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyDesignationFilter();
    }

    private void OnProductNameTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyProductNameFilter();
    }

    private void ApplyDesignationFilter()
    {
        var search = (DesignationEntry.Text ?? string.Empty).Trim();

        var filtered = _products
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Designation) &&
                (string.IsNullOrWhiteSpace(search) ||
                 x.Designation.Contains(search, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.Designation)
            .ToList();

        DesignationResultsList.ItemsSource = filtered;
        DesignationResultsBorder.IsVisible = filtered.Count > 0 && DesignationEntry.IsFocused;
    }

    private void ApplyProductNameFilter()
    {
        var search = (ProductNameEntry.Text ?? string.Empty).Trim();

        var filtered = _products
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Name) &&
                (string.IsNullOrWhiteSpace(search) ||
                 x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.Name)
            .ToList();

        ProductNameResultsList.ItemsSource = filtered;
        ProductNameResultsBorder.IsVisible =
            filtered.Count > 0 &&
            ProductNameEntry.IsFocused &&
            ProductNameEntry.IsEnabled;
    }

    private void OnDesignationSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Product product)
            return;

        DesignationEntry.Text = product.Designation;
        ProductNameEntry.Text = product.Name;

        var customer = _customers.FirstOrDefault(x => x.Id == product.CustomerId);
        _selectedCustomer = customer;
        CustomerSearchEntry.Text = customer?.Name ?? string.Empty;

        DesignationResultsBorder.IsVisible = false;
        ProductNameResultsBorder.IsVisible = false;

        DesignationResultsList.SelectedItem = null;

        SetProductFieldsEditable(true);
    }

    private void OnProductNameSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Product product)
            return;

        ProductNameEntry.Text = product.Name;
        DesignationEntry.Text = product.Designation;

        var customer = _customers.FirstOrDefault(x => x.Id == product.CustomerId);
        _selectedCustomer = customer;
        CustomerSearchEntry.Text = customer?.Name ?? string.Empty;

        DesignationResultsBorder.IsVisible = false;
        ProductNameResultsBorder.IsVisible = false;

        ProductNameResultsList.SelectedItem = null;

        SetProductFieldsEditable(true);
    }

    private void OnCustomerSearchFocused(object sender, FocusEventArgs e)
    {
        ApplyCustomerFilter();
    }

    private void OnDeveloperSearchFocused(object sender, FocusEventArgs e)
    {
        ApplyDeveloperFilter();
    }

    private void OnCustomerSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_selectedCustomer != null &&
            string.Equals(e.NewTextValue, _selectedCustomer.Name, StringComparison.Ordinal))
        {
            return;
        }

        _selectedCustomer = null;
        ApplyCustomerFilter();
    }

    private void OnDeveloperSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_selectedDeveloper != null &&
            string.Equals(e.NewTextValue, _selectedDeveloper.FullName, StringComparison.Ordinal))
        {
            return;
        }

        _selectedDeveloper = null;
        ApplyDeveloperFilter();
    }

    private void ApplyCustomerFilter()
    {
        var search = (CustomerSearchEntry.Text ?? string.Empty).Trim();

        var filtered = _customers
            .Where(x => string.IsNullOrWhiteSpace(search) ||
                        x.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        CustomerResultsList.ItemsSource = filtered;
        CustomerResultsBorder.IsVisible = filtered.Count > 0 && CustomerSearchEntry.IsEnabled;
    }

    private void ApplyDeveloperFilter()
    {
        var search = (DeveloperSearchEntry.Text ?? string.Empty).Trim();

        var filtered = _developers
            .Where(x => string.IsNullOrWhiteSpace(search) ||
                        x.FullName.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        DeveloperResultsList.ItemsSource = filtered;
        DeveloperResultsBorder.IsVisible = filtered.Count > 0;
    }

    private void OnCustomerSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Customer customer)
            return;

        _selectedCustomer = customer;
        CustomerSearchEntry.Text = customer.Name;
        CustomerResultsBorder.IsVisible = false;
        CustomerResultsList.SelectedItem = null;
    }

    private void OnDeveloperSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Developer developer)
            return;

        _selectedDeveloper = developer;
        DeveloperSearchEntry.Text = developer.FullName;
        DeveloperResultsBorder.IsVisible = false;
        DeveloperResultsList.SelectedItem = null;
    }

    private async void OnAddJournalEntry(object sender, EventArgs e)
    {
        try
        {
            if (!await _accessGuard.EnsureWriteAccessAsync(this))
                return;

            if (_selectedCustomer is not Customer customer)
            {
                await DisplayAlert("Ошибка", "Выберите клиента.", "OK");
                return;
            }

            if (_selectedDeveloper is not Developer developer)
            {
                await DisplayAlert("Ошибка", "Выберите разработчика.", "OK");
                return;
            }

            if (KitTypePicker.SelectedIndex < 0)
            {
                await DisplayAlert("Ошибка", "Выберите тип комплекта.", "OK");
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

            await LoadDataAsync();
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

        _selectedCustomer = null;
        _selectedDeveloper = null;

        CustomerSearchEntry.Text = string.Empty;
        DeveloperSearchEntry.Text = string.Empty;

        DesignationResultsBorder.IsVisible = false;
        ProductNameResultsBorder.IsVisible = false;
        CustomerResultsBorder.IsVisible = false;
        DeveloperResultsBorder.IsVisible = false;

        IssueDatePicker.Date = DateTime.Today;
        KitTypePicker.SelectedIndex = 0;

        SetProductFieldsEditable(true);
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
        ResetForm();
    }
}
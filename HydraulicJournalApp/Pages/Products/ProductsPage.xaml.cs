using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class ProductsPage : ContentPage
{
    private readonly DatabaseService _db;
    private List<ProductListItem> _allProducts = new();
    private readonly AccessGuardService _accessGuard;

    public ProductsPage(DatabaseService db, AccessGuardService accessGuard)
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
        _allProducts = await _db.GetProductListAsync();
        TotalProductsLabel.Text = $"Всего изделий: {_allProducts.Count}";
        ApplyFilter();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var search = (ProductSearchEntry.Text ?? string.Empty).Trim();

        var filtered = _allProducts
            .Where(x =>
                string.IsNullOrWhiteSpace(search) ||
                x.Designation.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Designation)
            .ThenBy(x => x.CustomerName)
            .ToList();

        ProductsList.ItemsSource = filtered;
    }

    private async void OnProductSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection.FirstOrDefault() is not ProductListItem product)
                return;

            ProductsList.SelectedItem = null;

            var designation = Uri.EscapeDataString(product.Designation);
            await Shell.Current.GoToAsync($"{nameof(ProductDetailsPage)}?designation={designation}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnDeleteProductClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button || button.CommandParameter is not string designation)
                return;

            var confirmed = await DisplayAlert(
                "Подтверждение",
                $"Удалить изделие \"{designation}\"?",
                "Удалить",
                "Отмена");

            if (!confirmed)
                return;

            if (!await _accessGuard.EnsureWriteAccessAsync(this))
                return;

            await _db.DeleteProductsByDesignationAsync(designation);
            await LoadDataAsync();

            await DisplayAlert("Готово", "Изделие удалено.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}
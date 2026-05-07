using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class ProductsPage : ContentPage
{
    private readonly DatabaseService _db;
    private List<ProductListItem> _allProducts = new();

    public ProductsPage(DatabaseService db)
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
        _allProducts = await _db.GetProductListAsync();
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
}
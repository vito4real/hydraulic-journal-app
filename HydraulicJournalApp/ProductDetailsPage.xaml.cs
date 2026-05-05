using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

[QueryProperty(nameof(Designation), "designation")]
public partial class ProductDetailsPage : ContentPage
{
    private readonly DatabaseService _db;

    private string _designation = string.Empty;

    public string Designation
    {
        get => _designation;
        set
        {
            _designation = Uri.UnescapeDataString(value ?? string.Empty);
            _ = LoadDataAsync();
        }
    }

    public ProductDetailsPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_designation))
                return;

            var details = await _db.GetProductDetailsByDesignationAsync(_designation);

            if (details == null)
            {
                await DisplayAlert("Ошибка", "Изделие не найдено.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            DesignationLabel.Text = details.Designation;
            ProductNamesLabel.Text = $"Наименования: {details.ProductNamesDisplay}";

            CustomersList.ItemsSource = details.Customers;
            JournalEntriesList.ItemsSource = details.JournalEntries;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}
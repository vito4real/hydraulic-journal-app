using System.Globalization;
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

    private async void OnSetDocumentationIssuedDateClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button || button.CommandParameter is not int journalEntryId)
                return;

            var input = await DisplayPromptAsync(
                "Дата выдачи комплекта КД",
                "Введите дату в формате dd.MM.yyyy",
                accept: "Сохранить",
                cancel: "Отмена",
                placeholder: "24.04.2026",
                initialValue: DateTime.Today.ToString("dd.MM.yyyy"));

            if (string.IsNullOrWhiteSpace(input))
                return;

            if (!DateTime.TryParseExact(
                    input.Trim(),
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                await DisplayAlert("Ошибка", "Введите дату в формате dd.MM.yyyy.", "OK");
                return;
            }

            await _db.SetDocumentationIssuedDateAsync(journalEntryId, date);
            await LoadDataAsync();

            await DisplayAlert("Готово", "Дата выдачи комплекта КД сохранена.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}
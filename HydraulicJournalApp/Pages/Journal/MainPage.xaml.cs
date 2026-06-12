using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AccessGuardService _accessGuard;
    private List<JournalEntryListItem> _allJournalEntries = new();

    public MainPage(
    DatabaseService db,
    AccessGuardService accessGuard)
    {
        InitializeComponent();

        _db = db;
        _accessGuard = accessGuard;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadJournalAsync();
    }

    private async Task LoadJournalAsync()
    {
        _allJournalEntries = await _db.GetJournalEntriesAsync();
        TotalJournalEntriesLabel.Text = $"Всего записей: {_allJournalEntries.Count}";
        ApplyFilters();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var designationFilter = (DesignationSearchEntry.Text ?? string.Empty).Trim();
        var customerFilter = (CustomerSearchEntry.Text ?? string.Empty).Trim();
        var developerFilter = (DeveloperSearchEntry.Text ?? string.Empty).Trim();

        var filtered = _allJournalEntries.Where(x =>
            (string.IsNullOrWhiteSpace(designationFilter) ||
             x.Designation.Contains(designationFilter, StringComparison.OrdinalIgnoreCase) ||
             x.ProductName.Contains(designationFilter, StringComparison.OrdinalIgnoreCase))
            &&
            (string.IsNullOrWhiteSpace(customerFilter) ||
             x.CustomerName.Contains(customerFilter, StringComparison.OrdinalIgnoreCase))
            &&
            (string.IsNullOrWhiteSpace(developerFilter) ||
             x.DeveloperName.Contains(developerFilter, StringComparison.OrdinalIgnoreCase))
        )
        .OrderBy(x => ExtractFirstNumberFromDesignation(x.Designation))
        .ThenBy(x => x.Designation)
        .ToList();

        JournalList.ItemsSource = filtered;
    }

    private static int ExtractFirstNumberFromDesignation(string designation)
    {
        if (string.IsNullOrWhiteSpace(designation))
            return int.MaxValue;

        var match = System.Text.RegularExpressions.Regex.Match(designation, @"\d+");

        if (!match.Success)
            return int.MaxValue;

        return int.TryParse(match.Value, out var number)
            ? number
            : int.MaxValue;
    }

    private async void OnJournalRowDoubleTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (e.Parameter is not string designation || string.IsNullOrWhiteSpace(designation))
                return;

            var encodedDesignation = Uri.EscapeDataString(designation);
            await Shell.Current.GoToAsync($"{nameof(ProductDetailsPage)}?designation={encodedDesignation}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private void OnResetFiltersClicked(object sender, EventArgs e)
    {
        DesignationSearchEntry.Text = string.Empty;
        CustomerSearchEntry.Text = string.Empty;
        DeveloperSearchEntry.Text = string.Empty;

        ApplyFilters();
    }

    private async void OnDeleteJournalEntryClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button || button.CommandParameter is not int journalEntryId)
                return;

            var confirmed = await DisplayAlert(
                "Подтверждение",
                "Удалить эту запись из журнала?",
                "Удалить",
                "Отмена");

            if (!confirmed)
                return;

            if (!await _accessGuard.EnsureWriteAccessAsync(this))
                return;

            await _db.DeleteJournalEntryAsync(journalEntryId);
            await LoadJournalAsync();

            await DisplayAlert("Готово", "Запись удалена.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}
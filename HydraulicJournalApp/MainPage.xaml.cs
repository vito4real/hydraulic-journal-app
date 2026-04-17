using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _db;
    private List<JournalEntryListItem> _allJournalEntries = new();

    public MainPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadJournalAsync();
    }

    private async Task LoadJournalAsync()
    {
        _allJournalEntries = await _db.GetJournalEntriesAsync();
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
             x.Designation.Contains(designationFilter, StringComparison.OrdinalIgnoreCase))
            &&
            (string.IsNullOrWhiteSpace(customerFilter) ||
             x.CustomerName.Contains(customerFilter, StringComparison.OrdinalIgnoreCase))
            &&
            (string.IsNullOrWhiteSpace(developerFilter) ||
             x.DeveloperName.Contains(developerFilter, StringComparison.OrdinalIgnoreCase))
        ).OrderBy(x => x.Designation).ToList();

        JournalList.ItemsSource = filtered;
    }

    private void OnResetFiltersClicked(object sender, EventArgs e)
    {
        DesignationSearchEntry.Text = string.Empty;
        CustomerSearchEntry.Text = string.Empty;
        DeveloperSearchEntry.Text = string.Empty;

        ApplyFilters();
    }
}
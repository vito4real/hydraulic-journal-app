using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _db;

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
        JournalList.ItemsSource = await _db.GetJournalEntriesAsync();
    }
}
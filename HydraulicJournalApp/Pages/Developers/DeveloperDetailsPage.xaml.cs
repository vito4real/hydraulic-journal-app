using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

[QueryProperty(nameof(DeveloperId), "developerId")]
public partial class DeveloperDetailsPage : ContentPage
{
    private readonly DatabaseService _db;
    private int _developerId;

    public string DeveloperId
    {
        set
        {
            if (int.TryParse(value, out var id))
            {
                _developerId = id;
            }
        }
    }

    public DeveloperDetailsPage(DatabaseService db)
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
        if (_developerId <= 0)
            return;

        var developer = await _db.GetDeveloperByIdAsync(_developerId);
        DeveloperNameLabel.Text = developer?.FullName ?? "Разработчик";

        DeveloperProductsList.ItemsSource = await _db.GetProductsByDeveloperAsync(_developerId);
    }
}
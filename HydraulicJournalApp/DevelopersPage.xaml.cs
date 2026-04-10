using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class DevelopersPage : ContentPage
{
    private readonly DatabaseService _db;

    public DevelopersPage(DatabaseService db)
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
        DevelopersList.ItemsSource = await _db.GetDevelopersAsync();
    }

    private async void OnAddDeveloper(object sender, EventArgs e)
    {
        try
        {
            await _db.AddDeveloperAsync(DeveloperEntry.Text ?? string.Empty);
            DeveloperEntry.Text = string.Empty;
            await LoadDataAsync();
            await DisplayAlert("Готово", "Разработчик добавлен.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}
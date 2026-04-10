using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class DevelopersPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AccessGuardService _accessGuard;

    public DevelopersPage(DatabaseService db, AccessGuardService accessGuard)
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
        DevelopersList.ItemsSource = await _db.GetDevelopersAsync();
    }

    private async void OnAddDeveloper(object sender, EventArgs e)
    {
        try
        {
            if (!await _accessGuard.EnsureWriteAccessAsync(this))
                return;

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

    private async void OnDeveloperSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection.FirstOrDefault() is not HydraulicJournalApp.Models.Developer developer)
                return;

            DevelopersList.SelectedItem = null;

            await Shell.Current.GoToAsync($"{nameof(DeveloperDetailsPage)}?developerId={developer.Id}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}
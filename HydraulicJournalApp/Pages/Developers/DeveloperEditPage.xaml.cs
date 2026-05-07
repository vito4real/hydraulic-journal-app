using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

[QueryProperty(nameof(DeveloperId), "developerId")]
public partial class DeveloperEditPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AccessGuardService _accessGuard;

    public int DeveloperId { get; set; }

    public DeveloperEditPage(DatabaseService db, AccessGuardService accessGuard)
    {
        InitializeComponent();
        _db = db;
        _accessGuard = accessGuard;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var developer = await _db.GetDeveloperByIdAsync(DeveloperId);

        if (developer == null)
        {
            await DisplayAlert("Ошибка", "Разработчик не найден.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        DeveloperNameEntry.Text = developer.FullName;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (!await _accessGuard.EnsureWriteAccessAsync(this))
                return;

            await _db.UpdateDeveloperAsync(DeveloperId, DeveloperNameEntry.Text ?? string.Empty);

            await DisplayAlert("Готово", "Разработчик обновлён.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
using HydraulicJournalApp.Services;

namespace HydraulicJournalApp;

public partial class App : Application
{
    private readonly DatabaseService _db;

    public App(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await _db.InitAsync();
        });

        return window;
    }
}
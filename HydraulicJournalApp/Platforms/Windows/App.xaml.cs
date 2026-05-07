using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace HydraulicJournalApp.WinUI;

public partial class App : MauiWinUIApplication
{
    private const int SW_MAXIMIZE = 3;

    public App()
    {
        InitializeComponent();
    }

    protected override MauiApp CreateMauiApp()
    {
        return MauiProgram.CreateMauiApp();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();

        if (mauiWindow?.Handler?.PlatformView is not Microsoft.UI.Xaml.Window nativeWindow)
            return;

        var hWnd = WindowNative.GetWindowHandle(nativeWindow);

        ShowWindow(hWnd, SW_MAXIMIZE);
    }

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
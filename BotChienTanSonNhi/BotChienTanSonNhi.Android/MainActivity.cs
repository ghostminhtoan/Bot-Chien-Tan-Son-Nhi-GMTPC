using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Avalonia;
using Avalonia.Android;

namespace BotChienTanSonNhi.Android;

[Activity(
    Label = "Bột Chiên Tân Sơn Nhì",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    ResizeableActivity = true,
    Exported = true,
    WindowSoftInputMode = SoftInput.AdjustResize,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.KeyboardHidden | ConfigChanges.Density | ConfigChanges.FontScale)]
public class MainActivity : AvaloniaMainActivity
{
    public static MainActivity? CurrentInstance { get; private set; }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        CurrentInstance = this;
        base.OnCreate(savedInstanceState);
        Window?.SetSoftInputMode(SoftInput.AdjustResize);

        // Bảo vệ toàn diện chống crash unhandled khi khởi động hoặc chạy ngầm
        AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            try
            {
                global::Android.Util.Log.Error("BotChien_Crash", $"[AndroidEnvironment Unhandled] {args.Exception}");
                args.Handled = true;
            }
            catch { }
        };
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            try
            {
                var ex = args.ExceptionObject as Exception;
                global::Android.Util.Log.Error("BotChien_Crash", $"[AppDomain Unhandled] {ex?.Message}\n{ex?.StackTrace}");
            }
            catch { }
        };
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            try
            {
                global::Android.Util.Log.Warn("BotChien_Task", $"[UnobservedTask] {args.Exception?.Message}");
                args.SetObserved();
            }
            catch { }
        };
    }
}

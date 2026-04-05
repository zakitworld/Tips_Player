using Android.App;
using Android.Runtime;

namespace Tips_Player
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            // Catch any unhandled .NET exception BEFORE it crosses the JNI boundary
            // and becomes a JavaProxyThrowable with no useful message.
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        protected override MauiApp CreateMauiApp()
        {
            try
            {
                return MauiProgram.CreateMauiApp();
            }
            catch (Exception ex)
            {
                WriteCrashLog(ex);
                global::Android.Util.Log.Error("TipsPlayer", $"FATAL: CreateMauiApp failed: {ex}");
                throw; // rethrow so Android still sees the crash (but now it's logged)
            }
        }

        private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            WriteCrashLog(ex);
            global::Android.Util.Log.Error("TipsPlayer", $"FATAL unhandled exception: {ex}");
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            WriteCrashLog(e.Exception);
            global::Android.Util.Log.Error("TipsPlayer", $"Unobserved task exception: {e.Exception}");
            e.SetObserved();
        }

        /// <summary>
        /// Writes crash details to /data/data/&lt;pkg&gt;/files/crash.txt
        /// so you can retrieve it via adb pull or Device File Explorer
        /// even after the process has died.
        /// </summary>
        private static void WriteCrashLog(Exception? ex)
        {
            try
            {
                var crashPath = System.IO.Path.Combine(
                    global::Android.App.Application.Context.FilesDir!.AbsolutePath,
                    "crash.txt");

                var content = $"[{DateTime.Now:O}] {ex}\n\n";
                System.IO.File.AppendAllText(crashPath, content);
            }
            catch
            {
                // best-effort — never throw from a crash handler
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Application = System.Windows.Application;

namespace Parot;

public partial class App : Application
{
    private static Mutex? _instanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        LogCrash("App.OnStartup started", null);

        try
        {
            // Kill any old, duplicate, or orphaned Parot processes so this active instance runs clean
            var current = Process.GetCurrentProcess();
            var existingProcesses = Process.GetProcessesByName(current.ProcessName)
                .Where(p => p.Id != current.Id)
                .ToList();

            foreach (var p in existingProcesses)
            {
                try
                {
                    LogCrash($"Terminating duplicate Parot process PID {p.Id}", null);
                    p.Kill();
                    p.WaitForExit(1000);
                }
                catch (Exception ex)
                {
                    LogCrash($"Could not terminate PID {p.Id}", ex);
                }
            }

            // Keep a local mutex for this session
            bool createdNew;
            _instanceMutex = new Mutex(true, "ParotDesktopCompanionSingleInstanceMutex", out createdNew);
        }
        catch (Exception ex)
        {
            LogCrash("Error during startup process cleanup", ex);
        }

        base.OnStartup(e);
        LogCrash("App.OnStartup base call completed", null);

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            LogCrash("AppDomain.UnhandledException", args.ExceptionObject as Exception);
        };

        DispatcherUnhandledException += (s, args) =>
        {
            LogCrash("DispatcherUnhandledException", args.Exception);
            args.Handled = true;
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _instanceMutex?.ReleaseMutex();
            _instanceMutex?.Dispose();
        }
        catch { }

        LogCrash($"OnExit called with exit code {e.ApplicationExitCode}", null);
        base.OnExit(e);
    }

    private static void LogCrash(string source, Exception? ex)
    {
        try
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Parot");
            Directory.CreateDirectory(appData);
            string path = Path.Combine(appData, "crash.log");
            string log = $"[{DateTime.Now}] {source}:\n{ex?.ToString()}\n\n";
            File.AppendAllText(path, log);
        }
        catch { }
    }
}

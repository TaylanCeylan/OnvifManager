using System.Diagnostics;
using System.Text;

namespace SPCConnector.Classes
{
#if LOGACTIVE
    public static class Logger
    {
        private static readonly object Sync = new();
        private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "spcconnector.log");

        public static void Initialize()
        {
            try
            {
                File.AppendAllText(LogPath, $"{Environment.NewLine}--- Session {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---{Environment.NewLine}");
                Application.ThreadException += Application_ThreadException;
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }
            catch
            {
            }
        }

        public static void Log(string line)
        {
            try
            {
                lock (Sync)
                    File.AppendAllText(LogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {line}{Environment.NewLine}");
            }
            catch
            {
            }
        }

        private static void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            string details = e.ExceptionObject is Exception exception ? GetExceptionMessages(exception) : e.ExceptionObject?.ToString() ?? "Bilinmeyen hata";
            Log($"Yakalanmamış hata (kapatılıyor: {e.IsTerminating}): {details}");
        }

        private static void Application_ThreadException(object? sender, ThreadExceptionEventArgs e)
        {
            Log($"Yakalanmamış arayüz hatası: {GetExceptionMessages(e.Exception)}");
        }

        private static string GetExceptionMessages(Exception exception)
        {
            var builder = new StringBuilder();
            for (Exception? current = exception; current != null; current = current.InnerException)
            {
                builder.AppendLine(current.Message);
                if (!string.IsNullOrWhiteSpace(current.StackTrace))
                    builder.AppendLine(current.StackTrace);
            }

            var frame = new StackTrace(exception, true).GetFrame(0);
            if (frame?.GetFileLineNumber() is int line && line > 0)
                builder.Append($"Satır: {line}");

            return builder.ToString();
        }
    }
#endif
}

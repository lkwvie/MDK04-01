using System;
using System.IO;
namespace ClassroomBooking.Services
{
    public static class SupportLogger
    {
        private static string LogDirectory { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClassroomBooking", "Logs"); } }
        public static string LogFilePath { get { return Path.Combine(LogDirectory, "application.log"); } }
        public static void Info(string message) { Write("INFO", message); }
        public static void Warning(string message) { Write("WARNING", message); }
        public static void Error(string message, Exception ex) { Write("ERROR", message + Environment.NewLine + ex); }
        private static void Write(string level, string message)
        {
            try { Directory.CreateDirectory(LogDirectory); File.AppendAllText(LogFilePath, string.Format("{0:yyyy-MM-dd HH:mm:ss} [{1}] {2}{3}", DateTime.Now, level, message, Environment.NewLine)); }
            catch { }
        }
    }
}

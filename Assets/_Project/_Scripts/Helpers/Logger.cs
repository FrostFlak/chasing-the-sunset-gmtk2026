namespace Helpers {
    public static class Log {
        public static void Debug(string message) => Unity.Logging.Log.Debug(message);
        public static void Info(string message) => Unity.Logging.Log.Info(message);
        public static void Warning(string message) => Unity.Logging.Log.Warning(message);
        public static void Error(string message) => Unity.Logging.Log.Error(message);
    }
}
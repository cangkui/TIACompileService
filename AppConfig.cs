using System.Configuration;

namespace TiaCompilerCLI.Configuration
{
    public static class AppConfig
    {
        
        public static string Get(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static string Get(string key, string defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public static bool GetBoolean(string key, bool defaultValue = false)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
            return defaultValue;
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}